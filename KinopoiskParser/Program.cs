using System;
using System.Configuration;
using System.Threading;

namespace KinopoiskParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var constants = new AppConstants();
            var historyWorker = new HistoryWorker();
            var logger = new Logger();
            using (var br = new Browser(constants))
            {
                var queueWorker = new QueueWorker(historyWorker, constants, br, logger);
                while (true)
                {
                    queueWorker.ProccessNextItem();
                }
            }
        }
    }
}
