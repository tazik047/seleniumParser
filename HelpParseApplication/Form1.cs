using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using KinopoiskParser;

namespace HelpParseApplication
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var lines = File.ReadAllLines("films.txt");
			dataGridView1.Rows.Clear();
			foreach (var line in lines)
			{
				var film = Regex.Match(line, @"name\|(([^\|]+\|?[^\|]*)+)\|\|").Groups[1].Value;
				string kinopoiskId = null;
				if (line.Contains("kinopoisk_id|"))
				{
					var t = line.Split(new[] {"kinopoisk_id|"}, StringSplitOptions.RemoveEmptyEntries);
					if (t.Length > 2)
					{
						var a = 10;
					}
					kinopoiskId = new string(t[1].TakeWhile(char.IsDigit).ToArray());
				}
				dataGridView1.Rows.Add(film, kinopoiskId);
			}

			Task.Run(()=>LoadIds());
		}

		private void LoadIds()
		{
			var constants = new AppConstants();
			using (var br = new Browser(constants))
			{
				foreach (DataGridViewRow dataGridViewRow in dataGridView1.Rows)
				{
					if (dataGridViewRow.Cells[1].Value == null || string.IsNullOrEmpty(dataGridViewRow.Cells[1].Value.ToString()))
					{
						br.FindFilm(dataGridViewRow.Cells[0].Value.ToString());
						var film = br.GetOpenedFilm(false);
						dataGridViewRow.Cells[2].Value = film.KinopoiskId;
					}
					else
					{
						dataGridViewRow.Cells[2].Value = dataGridViewRow.Cells[1].Value;
					}
				}
				
			}
		}
	}
}
