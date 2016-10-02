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
                var kinoManiac = new KinoManiac(constants, br);
                kinoManiac.SaveFilm(new Film());
                /*var queueWorker = new QueueWorker(historyWorker, constants, br, logger, kinoManiac);
                while (true)
                {
                    queueWorker.ProccessNextItem();
                }*/
            }
        }
    }
}
