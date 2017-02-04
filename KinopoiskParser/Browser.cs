using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;

namespace KinopoiskParser
{
	public class Browser : IDisposable
	{
		private static string GoogleUrl
		{
			get { return "https://www.google.com.ua"; }
		}

		private readonly ChromeDriver _chrome;
		private readonly AppConstants _appConstants;

		public Browser(AppConstants appConstants)
		{
			_appConstants = appConstants;
			var option = new ChromeOptions();
			option.AddArgument(string.Format("load-extension={0}\\AdBlock", AppDomain.CurrentDomain.BaseDirectory));
			_chrome = new ChromeDriver(option);
			_chrome.Manage().Window.Maximize();
			var newTabInstance = _chrome.WindowHandles[_chrome.WindowHandles.Count - 1];
			// switch our WebDriver to the new tab's window handle
			_chrome.SwitchTo().Window(newTabInstance);
			var t = _chrome.ExecuteJavaScript<object>("window.close();", null);
			_chrome.SwitchTo().Window(_chrome.WindowHandles[0]);
		}

		public void FindFilm(string name)
		{
			name = Uri.EscapeDataString(name);
			var url = string.Format("{1}/#q=" + _appConstants.KinopoiskSearchGoogle, name, GoogleUrl);
			GoToUrl(url);
			var xpath = By.XPath(".//div[@role='main']//a/../..//cite");
			_chrome.ElementIsVisible(xpath);
			var rawFilmLink = _chrome.FindElements(xpath).FirstOrDefault(p => !p.Text.Contains("video"));
			if (rawFilmLink == null)
			{
				throw new FilmNotFoundException();
			}

			var cleanFilmLink = Regex.Match(rawFilmLink.Text, "^[^w]+" + _appConstants.KinopoiskUrl + "\\/film\\/[^\\/]+").Value;

			GoToUrl(cleanFilmLink);
		}

		public void FindFilmById(string id)
		{
			var url = string.Format("https://{0}/film/{1}", _appConstants.KinopoiskUrl, id);
			GoToUrl(url);
		}

		public Film GetOpenedFilm(bool getTrailer = true)
		{
			if (_chrome.PageSource.Contains("404 - Страница не найдена"))
			{
				throw new FilmNotFoundException();
			}

			var film = new Film
			{
				KinopoiskId = Regex.Match(_chrome.Url, "\\/film\\/([^\\/]+)").Groups[1].Value,
				Title = _chrome.Title.Split(new[] { "— КиноПоиск" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim(),
				PublishYear = GetFieldValue("год"),
				Genres = GetFieldValues("жанр"),
				Slogan = GetFieldValue("слоган"),
				Countries = GetFieldValues("страна"),
				Producers = GetFieldValues("режиссер"),
				Actors = GetActors(),
				Duration = new string(GetFieldValue("время").TakeWhile(p => p != '.').ToArray()) + ".",
				Is18Plus = GetFieldValue("возраст").Contains("18")
			};

			ParsePosterAndFullName(film);
			GetDescription(film);
			GetFilmType(film);

			if (getTrailer)
			{
				GetTrailer(film);
			}

			/*var json = Newtonsoft.Json.JsonConvert.SerializeObject(film, Formatting.Indented).Replace("\r\n", "<br/>");
            _chrome.ExecuteScript("document.getElementsByTagName('body')[0].innerHTML = '<div style=\"max - width: 100 %;margin: 40px;background: white;\">" + json + "</div>'");
            Thread.Sleep(10000);*/
			return film;
		}

		private void GetFilmType(Film film)
		{
			try
			{
				var element = _chrome.FindElementByXPath(".//*[text()=\"Рейтинг фильма\"]");
				film.FilmType = FilmType.Film;
			}
			catch (NoSuchElementException)
			{
				try
				{
					var element = _chrome.FindElementByXPath(".//*[text()=\"Рейтинг сериала\"]");
					film.FilmType = FilmType.Serial;
				}
				catch (NoSuchElementException)
				{
					try
					{
						var element = _chrome.FindElementByXPath(".//*[text()=\"Рейтинг мультсериала\"]");
						film.FilmType = FilmType.Anime;
					}
					catch (NoSuchElementException)
					{
						film.FilmType = FilmType.Other;
					}
				}
			}
		}

		private void GetTrailer(Film film)
		{
			try
			{
				var url = string.Format("https://www.{1}/results?search_query={0} русский трейлер", film.Title,
					_appConstants.YouTubeUrl);
				GoToUrl(url);
				var xpath = By.XPath(".//a[contains(@href,'watch')]");
				_chrome.ElementIsVisible(xpath, TimeSpan.FromMinutes(1));
				film.Trailer = _chrome.FindElements(xpath).First().GetAttribute("href");
				Uri myUri = new Uri(film.Trailer);
				film.Trailer = HttpUtility.ParseQueryString(myUri.Query).Get("v");
			}
			catch (WebDriverTimeoutException)
			{
				Console.WriteLine("Trailer for film {0} not found.", film.FullName);
				film.Trailer = string.Empty;
			}
		}

		private void GetDescription(Film film)
		{
			try
			{
				film.Description = _chrome.FindElement(By.XPath(_appConstants.DescriptionSection)).GetAttribute("innerHTML");
				return;
			}
			catch (NoSuchElementException e)
			{
				Console.WriteLine("{0}: {1}", film.FullName, e.Message);
			}

			film.Description = string.Empty;
		}

		private string[] GetActors()
		{
			var items =
				_chrome.FindElements(
					By.XPath(".//*[contains(text(),'В главных ролях')]/following-sibling::*[1]//*[contains(@itemprop, 'actors')]"));
			return items
				.Select(p => p.Text.Trim())
				.Where(p => p != "...")
				.ToArray();
		}

		private void ParsePosterAndFullName(Film film)
		{
			var xpathFullName = string.Format(".//img[contains(@alt,'{0}')]", film.Title.Split().First());

			film.Poster = _chrome.FindElement(By.XPath(xpathFullName))
				.GetAttribute("src")
				.Replace("film_iphone/iphone360_", "film_big/");
			film.FullName = _chrome.FindElement(By.XPath(xpathFullName)).GetAttribute("alt");
			var name = _chrome.FindElements(By.XPath(".//*[@itemprop='name']")).FirstOrDefault();
			if (name != null)
			{
				film.Title = name.Text;
			}
		}

		private string[] GetFieldValues(string name)
		{
			return GetFieldValue(name)
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Trim())
				.Where(p => p != "слова" && !p.Contains("..."))
				.ToArray();
		}

		private string GetFieldValue(string name)
		{
			try
			{
				var xpath = string.Format(".//*[text()='{0}']/..", name);
				Actions action = new Actions(_chrome);
				action.MoveToElement(_chrome.FindElement(By.XPath(xpath))).Perform();
				return
					_chrome.FindElement(By.XPath(xpath)).Text.Split(new[] { name }, StringSplitOptions.RemoveEmptyEntries)
						[0].Trim();
			}
			catch (NoSuchElementException)
			{
				return "";
			}
		}

		public void GoToUrl(string url)
		{
			_chrome.Navigate().GoToUrl(url);
		}

		public void Dispose()
		{
			_chrome.Quit();
		}

		public void Click(By xPath)
		{
			_chrome.FindElement(xPath).Click();
		}

		public void SetValue(By xPath, string value)
		{
			_chrome.FindElement(xPath).SendKeys(value);
		}

		public void Clear(By xPath)
		{
			_chrome.FindElement(xPath).Clear();
		}

		public string GetValue(By xPath)
		{
			return _chrome.FindElement(xPath).GetAttribute("value");
		}

		public void SetValueWithEnter(By xPath, string value)
		{
			_chrome.FindElement(xPath).Clear();
			SetValue(xPath, value);
			_chrome.FindElement(xPath).SendKeys(Keys.Return);
		}

		public void SetValueWithEnter(By xPath, params string[] values)
		{
			foreach (var value in values)
			{
				SetValueWithEnter(xPath, value);
			}
		}

		public void SetValueInIframe(string frameId, By xPath, string url)
		{
			SetValueInIframe(By.Id(frameId), xPath, url);
		}

		public void SetValueInIframe(By frame, By xPath, string url)
		{
			DoActionInFrame(frame, () => SetValue(xPath, url));
		}

		public void SetHtml(string jsAccessor, string value)
		{
			_chrome.ExecuteScript(string.Format("{0}.innerHTML = '{1}'", jsAccessor, value));
		}

		public void SetHtmlInFrame(string frameId, string jsAccessor, string value)
		{
			DoActionInFrame(By.Id(frameId), () => SetHtml(jsAccessor, value));
		}

		public void ClickInFrame(string frameId, By xPath)
		{
			ClickInFrame(By.Id(frameId), xPath);
		}

		public void ClickInFrame(By frame, By xPath)
		{
			DoActionInFrame(frame, () => Click(xPath));
		}

		public bool Contain(By xPath)
		{
			return _chrome.FindElements(xPath).Any();
		}

		private void DoActionInFrame(By frame, Action action)
		{
			_chrome.SwitchTo().Frame(_chrome.FindElement(frame));
			action();
			_chrome.SwitchTo().DefaultContent();
		}

		public void WaitUntilHasntClass(By id, string className)
		{
			WaitExtensions.Until(() => _chrome.FindElement(id).GetAttribute("class").Contains(className));
		}

		public void WaitUntilHasntClassInFrame(string frameId, By id, string className)
		{
			DoActionInFrame(By.Id(frameId), () => WaitUntilHasntClass(id, className));
		}

		public void WaitUntilIsntVisible(By xPath)
		{
			WaitExtensions.Until(() => _chrome.FindElement(xPath).Displayed);
		}
	}
}