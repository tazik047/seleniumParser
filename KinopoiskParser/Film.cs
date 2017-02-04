using System.Linq;
using System.Text.RegularExpressions;

namespace KinopoiskParser
{
	public class Film
	{
		private string _slogan;
		private string _title;
		private string[] _genres;
		private string _publishYear;

		public Film()
		{
			Quality = "HD";
		}

		public string KinopoiskId { get; set; }

		public string Title
		{
			get
			{
				if (FilmType == FilmType.Serial || FilmType == FilmType.Anime)
				{
					return Regex.Replace(_title, @"\([.]*сериал[^)]*\)", "").Trim();
				}

				return _title;
			}
			set { _title = value; }
		}

		public string FullName { get; set; }

		public string Poster { get; set; }

		public string Trailer { get; set; }

		public string Slogan
		{
			get { return _slogan == "-" ? string.Empty : _slogan; }
			set { _slogan = value.Trim(); }
		}

		public string PublishYear
		{
			get
			{
				if (FilmType == FilmType.Serial || FilmType == FilmType.Anime)
				{
					return new string(_publishYear.TakeWhile(p => p != '(').ToArray()).Trim();
				}

				return _publishYear;
			}
			set { _publishYear = value; }
		}

		public string[] Genres
		{
			get
			{
				if (FilmType == FilmType.Film)
				{
					return new[] { "Фильмы" }.Union(_genres).ToArray();
				}
				if (FilmType == FilmType.Serial)
				{
					return new[] { "Сериал" }.Union(_genres).ToArray();
				}

				return _genres;
			}
			set { _genres = value; }
		}

		public string[] Countries { get; set; }

		public string[] Producers { get; set; }

		public string[] Actors { get; set; }

		public string Description { get; set; }

		public string Quality { get; set; }

		public string Duration { get; set; }

		public bool Is18Plus { get; set; }

		public FilmType FilmType { get; set; }
	}
}