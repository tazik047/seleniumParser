namespace KinopoiskParser
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var br = new Browser())
            {
                br.FindFilm("Звездные войны 7");
                //br.FindFilm("Гарри Поттер 3");
                var f = br.GetOpenedFilm();
            }
        }
    }
}
