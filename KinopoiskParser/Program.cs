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
            //using (var br = new Browser(constants))
            {
                /*br.FindFilm("Звездные войны 7");
                //br.FindFilm("Гарри Поттер 3");
                var f = br.GetOpenedFilm();*/
                var t = "";
                while ((t = constants.QueueItem)!=null)
                {
                    Console.WriteLine(t);
                    Thread.Sleep(5000);
                }
                Console.WriteLine("end");
                Console.ReadLine();
            }
        }
    }
}
