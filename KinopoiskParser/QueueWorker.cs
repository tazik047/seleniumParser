using System;

namespace KinopoiskParser
{
    class QueueWorker
    {
        private readonly HistoryWorker _historyWorker;
        private readonly AppConstants _appConstants;
        private readonly Browser _browser;
        private readonly Logger _logger;

        public QueueWorker(HistoryWorker historyWorker, AppConstants appConstants, Browser browser, Logger logger)
        {
            _historyWorker = historyWorker;
            _appConstants = appConstants;
            _browser = browser;
            _logger = logger;
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

                _browser.SaveFilm(film);
                _historyWorker.AddToHistory(film.KinopoiskId);
            }
            catch (FilmNotFoundException)
            {
                _logger.LogMessage("Film with name {0} not found.", name);
            }
        }
    }
}
