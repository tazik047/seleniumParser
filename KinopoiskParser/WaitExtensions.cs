using System;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace KinopoiskParser
{
    public static class WaitExtensions
    {
        private static TimeSpan TimeoutDefault
        {
            get { return TimeSpan.FromMinutes(3); }
        }

        private static WebDriverWait For(IWebDriver driver, TimeSpan timeout)
        {
            var instance = new WebDriverWait(driver, timeout);
            instance.Timeout = timeout;
            instance.IgnoreExceptionTypes(typeof(NoSuchElementException));
            instance.IgnoreExceptionTypes(typeof(WebDriverException));
            return instance;
        }

        public static void ElementIsVisible(this IWebDriver driver, By locator)
        {
            driver.ElementIsVisible(locator, TimeoutDefault);
        }

        public static void ElementIsVisible(this IWebDriver driver, By locator, TimeSpan timeout)
        {
            For(driver, timeout).Until(ExpectedConditions.ElementIsVisible(locator));
        }

        public static void ElementIsClickable(this IWebDriver driver, By locator)
        {
            driver.ElementIsClickable(locator, TimeoutDefault);
        }

        public static void ElementIsClickable(this IWebDriver driver, By locator, TimeSpan timeout)
        {
            For(driver, timeout).Until(IsClickable(locator));
        }

        private static Func<IWebDriver, IWebElement> IsClickable(By locator)
        {
            return driver =>
            {
                var element = driver.FindElement(locator);
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            };
        }

        public static void ElementIsAbsent(this IWebDriver driver, By locator)
        {
            For(driver, TimeoutDefault).Until(d => d.FindElements(locator).Count == 0);
        }

        public static void Until(Func<bool> condition, TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout && !condition())
            {
                Thread.Sleep(1000);
            }
            // Either we finished the work or we ran out of time.
            if (!condition()) throw new TimeoutException();
        }

        public static void Until(Func<bool> condition)
        {
            Until(condition, TimeoutDefault);
        }
    }
}