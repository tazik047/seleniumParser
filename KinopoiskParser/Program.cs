
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
				//br.FindFilm("Страшная воля богов");
				//var film = br.GetOpenedFilm();
				var kinoManiac = new KinoManiac(constants, br);
				//kinoManiac.SaveFilm(film);
				var queueWorker = new QueueWorker(historyWorker, constants, br, logger, kinoManiac);
				while (true)
				{
					queueWorker.ProccessNextItem();
				}
			}
		}
	}
}
