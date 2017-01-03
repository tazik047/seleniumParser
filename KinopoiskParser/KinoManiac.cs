using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace KinopoiskParser
{
	class KinoManiac
	{
		private readonly AppConstants _appConstants;
		private readonly Browser _browser;

		public KinoManiac(AppConstants appConstants, Browser browser)
		{
			_appConstants = appConstants;
			_browser = browser;
		}

		public void SaveFilm(Film film)
		{
			_browser.GoToUrl(_appConstants.KinoManiacUrl);
			Login();
			_browser.SetValue(By.XPath(".//input[@id='title']"), film.Title);
			_browser.SetValueWithEnter(By.XPath(".//*[@id='category_chzn']//input"), new [] { "Фильмы"}.Union(film.Genres).ToArray());
			LoadImage(film.Poster);
			SetDescription(film.Description);
			_browser.SetValue(By.Id("xf_name"), film.FullName);
			_browser.SetValueWithEnter(By.Id("xf_year-tokenfield"), film.PublishYear);
			_browser.SetValueWithEnter(By.Id("xf_strana-tokenfield"), film.Countries);
			_browser.SetValue(By.Id("xf_slogan"), film.Slogan);
			_browser.SetValueWithEnter(By.Id("xf_reziser-tokenfield"), film.Producers);
			_browser.SetValueWithEnter(By.Id("xf_akteri-tokenfield"), film.Actors);
			_browser.SetValueWithEnter(By.Id("xf_zanr-tokenfield"), film.Genres);
			_browser.SetValue(By.Id("xf_dlina"), film.Duration);
			_browser.SetValue(By.Id("xf_quality"), film.Quality);
			_browser.SetValue(By.Id("xf_youtube"), film.Trailer);
			if (film.Is18Plus)
			{
				_browser.SetValue(By.Id("xf_18"), "18+");
			}
			_browser.SetValue(By.Id("xf_kinopoisk_id"), film.KinopoiskId);

			_browser.Click(By.Id("hdlightFindButton"));

			_browser.WaitUntilIsntVisible(By.XPath(".//*[@id='hdlightFindResults']"));
			_browser.Click(By.XPath(".//*[@id='hdlightFindResults']//button[text()='Вставить ссылку']"));

			var additionalTab = By.XPath(".//div[@class='box-header']//ul[contains(@class,'nav')]/li[3]");
			_browser.Click(additionalTab);
			_browser.WaitUntilHasntClass(additionalTab, "active");
			_browser.SetValue(By.XPath(".//input[@name='meta_title']"), string.Format(_appConstants.MetaTitleFormat, film.Title, film.PublishYear));

			_browser.Click(By.XPath(".//*[@id='addnews']/div[@class='padded']//input[@type='submit']"));
		}

		private void SetDescription(string description)
		{
			_browser.Click(By.Id("idContentoEdit1"));
			_browser.SetHtmlInFrame("idContentoEdit1", "document.getElementsByTagName('body')[0]", description);
		}

		private void LoadImage(string url)
		{
			var frameId = "mediauploadframe";
			_browser.Click(By.Id("idContentoEdit0"));
			_browser.Click(By.XPath(".//div[@id='oEdit0tbargrpEdit3']//table//tr[3]/td/table[4]"));
			_browser.SetValueInIframe(frameId, By.XPath(".//input[@id='copyurl']"), url);
			_browser.ClickInFrame(frameId, By.XPath(".//*[@id='stmode']//button[text()='Загрузить']"));
			_browser.WaitUntilHasntClassInFrame(frameId, By.Id("link2"), "current");
			_browser.ClickInFrame(frameId, By.XPath(".//*[@id='cont1']//img"));
			_browser.ClickInFrame(frameId, By.Id("ins_image"));
			_browser.Click(By.XPath(".//a[@role='button' and contains(@class, 'titlebar-close')]/span"));
		}

		private void Login()
		{
			if (_browser.Contain(By.XPath(".//div[@id='login-box']")))
			{
				_browser.SetValue(By.XPath(".//div[@id='login-box']//input[@name='username']"), _appConstants.Login);
				_browser.SetValue(By.XPath(".//div[@id='login-box']//input[@name='password']"), _appConstants.Password);
				_browser.Click(By.XPath(".//div[@id='login-box']//input[@type='image']"));
			}
		}
	}
}
