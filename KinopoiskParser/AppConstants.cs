using System.Configuration;

namespace KinopoiskParser
{
    public class AppConstants
    {
        private string _kinopoiskUrl;
        private string _kinoManiacUrl;
        private string _descriptionSection;

        public string KinopoiskUrl
        {
            get { return _kinopoiskUrl ?? (_kinopoiskUrl = ConfigurationManager.AppSettings["KinopoiskUrl"]); }
        }

        public string KinoManiacUrl
        {
            get { return _kinoManiacUrl ?? (_kinoManiacUrl = ConfigurationManager.AppSettings["KinoManiacUrl"]); }
        }

        public string DescriptionSection
        {
            get { return _descriptionSection ?? (_descriptionSection = ConfigurationManager.AppSettings["DescriptionSection"]); }
        }
    }
}
