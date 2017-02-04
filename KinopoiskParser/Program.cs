
using System;

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
				var count = 0;
				var kinoManiac = new KinoManiac(constants, br);
				
				var queueWorker = new QueueWorker(historyWorker, constants, br, logger, kinoManiac);
				while (true)
				{
					count++;
					queueWorker.ProccessNextItem();
					if (constants.LimitCount != 0 && count == constants.LimitCount)
					{
						Console.WriteLine("Все, план на сегодня выполнен!");
						Console.ReadLine();
						break;
					}
				}
			}
		}
	}
}
