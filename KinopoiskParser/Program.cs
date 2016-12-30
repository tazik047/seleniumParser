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
				br.FindFilm("Гарри поттер и узник азкабана");
	            var film = br.GetOpenedFilm();
	            var kinoManiac = new KinoManiac(constants, br);
	            kinoManiac.SaveFilm(film);
	            /*var queueWorker = new QueueWorker(historyWorker, constants, br, logger, kinoManiac);
	            while (true)
	            {
	                queueWorker.ProccessNextItem();
	            }*/
            }
        }
    }
}
