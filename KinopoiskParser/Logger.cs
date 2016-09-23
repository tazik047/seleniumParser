using System;
using System.IO;

namespace KinopoiskParser
{
    public class Logger
    {
        private string _path = "_logs.txt";

        public void LogMessage(string format, params object[] items)
        {
            var text = string.Format(format, items);
            var message = string.Format("{0} - {1}", DateTime.Now, text);
            File.AppendAllLines(_path, new[] {message});
        }
    }
}