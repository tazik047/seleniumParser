using System;
using System.Threading;

namespace KinopoiskParser
{
	class QueueWorker
	{
		private readonly HistoryWorker _historyWorker;
		private readonly AppConstants _appConstants;
		private readonly Browser _browser;
		private readonly Logger _logger;
		private readonly KinoManiac _kinoManiac;

		public QueueWorker(HistoryWorker historyWorker, AppConstants appConstants, Browser browser, Logger logger, KinoManiac kinoManiac)
		{
			_historyWorker = historyWorker;
			_appConstants = appConstants;
			_browser = browser;
			_logger = logger;
			_kinoManiac = kinoManiac;
		}

		public void ProccessNextItem()
		{
			var preference = _appConstants.QueueItem;
			if (string.IsNullOrEmpty(preference))
			{
				preference = _appConstants.CurrentPosition.ToString();
				WorkWithFilm(_browser.FindFilmById, preference);
				_appConstants.CurrentPosition++;
			}
			else
			{
				WorkWithFilm(_browser.FindFilm, preference);
			}
			Thread.Sleep(_appConstants.WaitTimeout);
		}

		private void WorkWithFilm(Action<string> findFilm, string name)
		{
			try
			{
				findFilm(name);
				var film = _browser.GetOpenedFilm();
				if (_historyWorker.IsInHistory(film.KinopoiskId))
				{
					return;
				}

				_kinoManiac.SaveFilm(film);
				_historyWorker.AddToHistory(film.KinopoiskId);
			}
			catch (FilmNotFoundException)
			{
				_logger.LogMessage("Film with name {0} not found.", name);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Попробовать еще раз?(y/n)");
				var t = Console.ReadLine().ToLower();
				if (t == "y")
				{
					WorkWithFilm(findFilm, name);
				}
			}
		}
	}
}
