using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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

        public Browser()
        {
            _appConstants = new AppConstants();
            _chrome = new ChromeDriver();
            _chrome.Manage().Window.Maximize();
        }

        public void FindFilm(string name)
        {
            var url = string.Format("{0}/#q={1} site:{2}", GoogleUrl, name, _appConstants.KinopoiskUrl);
            GoToUrl(url);
            //var xpath = string.Format("(.//a[contains(@data-href,'{0}/film/')] | .//a[contains(@href, '{0}/film/')])/../..//cite", _appConstants.KinopoiskUrl);
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

        public Film GetOpenedFilm()
        {
            var film = new Film
            {
                Title = _chrome.Title.Split(new[] {"— КиноПоиск"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim(),
                PublishYear = GetFieldValue("год"),
                Genres = GetFieldValues("жанр"),
                Slogan = GetFieldValue("слоган"),
                Countries = GetFieldValues("страна"),
                Producers = GetFieldValues("режиссер"),
                Actors = GetActors(),
                Description = GetDescription()
            };
            ParsePosterAndFullName(film);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(film, Formatting.Indented).Replace("\r\n", "<br/>");
            _chrome.ExecuteScript("document.getElementsByTagName('body')[0].innerHTML = '<div style=\"max - width: 100 %;margin: 40px;background: white;\">" + json + "</div>'");
            Thread.Sleep(10000);
            return film;
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
            var xpathFullName = string.Format(".//img[contains(@alt,'{0}')]", film.Title);
            var xpathPoster = xpathFullName + "/..";

            film.Poster = _chrome.FindElement(By.XPath(xpathPoster)).GetAttribute("href");
            film.FullName = _chrome.FindElement(By.XPath(xpathFullName)).GetAttribute("alt");
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
            var xpath = string.Format(".//*[text()='{0}']/..", name);
            return _chrome.FindElement(By.XPath(xpath)).Text.Split(new[] {name}, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        private void GoToUrl(string url)
        {
            _chrome.Navigate().GoToUrl(url);
        }

        public void Dispose()
        {
            _chrome.Quit();
        }
    }

    public class Film
    {
        public string Title { get; set; }

        public string FullName { get; set; }

        public string Poster { get; set; }

        public string Slogan { get; set; }

        public string PublishYear { get; set; }
        
        public string[] Genres { get; set; }

        public string[] Countries { get; set; }

        public string[] Producers { get; set; }

        public string[] Actors { get; set; }

        public string Description { get; set; }
    }
}
