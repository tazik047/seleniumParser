using System.IO;
using System.Linq;

namespace KinopoiskParser
{
    class HistoryWorker
    {
        private readonly string _historyPath = "history.txt";

        public void AddToHistory(string id)
        {
            if (!IsInHistory(id))
            {
                File.AppendAllLines(_historyPath, new[] {id});
            }
        }

        public bool IsInHistory(string id)
        {
            if (!File.Exists(_historyPath))
            {
                return false;
            }

            return File.ReadAllLines(_historyPath).Contains(id);
        }
    }
}
