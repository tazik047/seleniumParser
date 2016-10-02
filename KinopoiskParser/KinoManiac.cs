using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
