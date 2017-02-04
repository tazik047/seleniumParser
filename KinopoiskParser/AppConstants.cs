using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace KinopoiskParser
{
	public class AppConstants
	{
		private string _kinopoiskUrl;
		private string _kinoManiacUrl;
		private string _descriptionSection;
		private string _youTubeUrl;
		private string _userQueueFile = "userPreferences.txt";
		private string _metaTitleFormat;
		private int? _waitTimeout;
		private string _kinopoiskSearchGoogle;

		public int WaitTimeout
		{
			get
			{
				if (_waitTimeout == null)
				{
					_waitTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["WaitTimeout"]);
				}

				return _waitTimeout.Value * 1000;
			}
		}

		public string KinopoiskUrl
		{
			get { return _kinopoiskUrl ?? (_kinopoiskUrl = ConfigurationManager.AppSettings["KinopoiskUrl"]); }
		}

		public string KinopoiskSearchGoogle
		{
			get { return _kinopoiskSearchGoogle ?? ( _kinopoiskSearchGoogle = ConfigurationManager.AppSettings["KinopoiskSearchGoogle"]); }
		}

		public string KinoManiacUrl
		{
			get { return _kinoManiacUrl ?? (_kinoManiacUrl = ConfigurationManager.AppSettings["KinoManiacUrl"]); }
		}

		public string DescriptionSection
		{
			get { return _descriptionSection ?? (_descriptionSection = ConfigurationManager.AppSettings["DescriptionSection"]); }
		}

		public string MetaTitleFormat
		{
			get { return _metaTitleFormat ?? (_metaTitleFormat = ConfigurationManager.AppSettings["MetaTitleFormat"]); }
		}

		public string YouTubeUrl
		{
			get { return _youTubeUrl ?? (_youTubeUrl = ConfigurationManager.AppSettings["YouTubeUrl"]); }
		}

		public long CurrentPosition
		{
			get
			{
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

				return long.Parse(config.AppSettings.Settings["CurrentPosition"].Value);
			}
			set
			{
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

				config.AppSettings.Settings["CurrentPosition"].Value = value.ToString();
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection("AppSettings");
			}
		}

		public string SeeQueueItem
		{
			get
			{
				if (!File.Exists(_userQueueFile))
				{
					return null;
				}
				var items = File.ReadAllText(_userQueueFile, Encoding.UTF8)
					.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				if (!items.Any())
				{
					return null;
				}

				return items.First();
			}
		}

		public string QueueItem
		{
			get
			{
				if (!File.Exists(_userQueueFile))
				{
					return null;
				}
				var items = File.ReadAllText(_userQueueFile, Encoding.UTF8)
					.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				if (!items.Any())
				{
					return null;
				}

				var first = items.First();
				var res =
					items.Skip(1)
						.Aggregate(new StringBuilder(), (p, i) => p.Append(i).Append(Environment.NewLine))
						.ToString();
				File.WriteAllText(_userQueueFile, res, Encoding.UTF8);

				return first.Trim();
			}
		}

		public string Login
		{
			get { return ConfigurationManager.AppSettings["Login"]; }
		}

		public string Password
		{
			get { return ConfigurationManager.AppSettings["Password"]; }
		}

		public int LimitCount
		{
			get { return int.Parse(ConfigurationManager.AppSettings["LimitCount"]); }
		}
	}
}
