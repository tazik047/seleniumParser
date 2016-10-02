using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace KinopoiskParser
{
    class Browser : IDisposable
    {
        private static string GoogleUrl
        {
            get { return "https://www.google.com.ua"; }
        }
        private readonly ChromeDriver _chrome;
        private readonly AppConstants _appConstants;
        private int _searchTabId = 0;
        private int _kinomaniacTabId = 1;

        public Browser(AppConstants appConstants)
        {
            _appConstants = appConstants;
            _chrome = new ChromeDriver();
            _chrome.Manage().Window.Maximize();
        }

        public void FindFilm(string name)
        {
            var url = string.Format("{0}/#q={1} site:{2}", GoogleUrl, name, _appConstants.KinopoiskUrl);
            GoToUrl(url);
            var xpath = By.XPath(".//a/../..//cite");
            _chrome.ElementIsVisible(xpath);
            var rawFilmLink = _chrome.FindElements(xpath).FirstOrDefault();
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

        public Film GetOpenedFilm()
        {
            if (_chrome.PageSource.Contains("404 - Страница не найдена"))
            {
                throw new FilmNotFoundException();
            }

            var film = new Film
            {
                KinopoiskId = Regex.Match(_chrome.Url, "\\/film\\/([^\\/]+)").Groups[1].Value,
                Title = _chrome.Title.Split(new[] {"— КиноПоиск"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim(),
                PublishYear = GetFieldValue("год"),
                Genres = GetFieldValues("жанр"),
                Slogan = GetFieldValue("слоган"),
                Countries = GetFieldValues("страна"),
                Producers = GetFieldValues("режиссер"),
                Actors = GetActors(),
                Description = GetDescription(),
                Duration = new string(GetFieldValue("время").TakeWhile(p=>p!='.').ToArray()) + ".",
                Is18Plus = GetFieldValue("возраст").Contains("18")
            };
            ParsePosterAndFullName(film);
            GetTrailer(film);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(film, Formatting.Indented).Replace("\r\n", "<br/>");
            _chrome.ExecuteScript("document.getElementsByTagName('body')[0].innerHTML = '<div style=\"max - width: 100 %;margin: 40px;background: white;\">" + json + "</div>'");
            Thread.Sleep(10000);
            return film;
        }

        private void GetTrailer(Film film)
        {
            var url = string.Format("https://www.{1}/results?search_query={0} руссикй трейлер", film.Title, _appConstants.YouTubeUrl);
            GoToUrl(url);
            var xpath = By.XPath(".//a[contains(@href,'watch')]");
            _chrome.ElementIsVisible(xpath);
            film.Trailer = _chrome.FindElements(xpath).First().GetAttribute("href");
            Uri myUri = new Uri(film.Trailer);
            film.Trailer = HttpUtility.ParseQueryString(myUri.Query).Get("v");
        }

        private string GetDescription()
        {
            return _chrome.FindElement(By.XPath(_appConstants.DescriptionSection)).GetAttribute("innerHTML");
        }

        private string[] GetActors()
        {
            var items = _chrome.FindElements(By.XPath(".//*[contains(text(),'В главных ролях')]/following-sibling::*[1]//*[contains(@itemprop, 'actors')]"));
            return items
                    .Select(p => p.Text.Trim())
                    .Where(p => p != "...")
                    .ToArray();
        }

        private void ParsePosterAndFullName(Film film)
        {
            var xpathFullName = string.Format(".//img[contains(@alt,'{0}')]", film.Title.Split().First());

            film.Poster = _chrome.FindElement(By.XPath(xpathFullName)).GetAttribute("src");
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
                .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
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
                    _chrome.FindElement(By.XPath(xpath)).Text.Split(new[] {name}, StringSplitOptions.RemoveEmptyEntries)
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

        public bool Contain(By xPath)
        {
            return _chrome.FindElements(xPath).Any();
        }
    }

    public class Film
    {
        public Film()
        {
            Quality = "HD";
        }

        public string KinopoiskId { get; set; }

        public string Title { get; set; }

        public string FullName { get; set; }

        public string Poster { get; set; }

        public string Trailer { get; set; }

        public string Slogan { get; set; }

        public string PublishYear { get; set; }
        
        public string[] Genres { get; set; }

        public string[] Countries { get; set; }

        public string[] Producers { get; set; }

        public string[] Actors { get; set; }

        public string Description { get; set; }

        public string Quality { get; set; }

        public string Duration { get; set; }

        public bool Is18Plus { get; set; }
    }
}
