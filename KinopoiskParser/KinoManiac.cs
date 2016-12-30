using System.Linq;
using System.Threading;
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
			foreach (var genre in film.Genres.Reverse())
			{
				_browser.SetValueWithEnter(By.XPath(".//*[@id='category_chzn']//input"), genre);
			}
			//LoadImage(film.Poster);
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
