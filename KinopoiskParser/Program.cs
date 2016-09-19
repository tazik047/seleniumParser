namespace KinopoiskParser
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var br = new Browser())
            {
                br.FindFilm("Звездные войны 7");
                var f = br.GetOpenedFilm();
            }
        }
    }
}
