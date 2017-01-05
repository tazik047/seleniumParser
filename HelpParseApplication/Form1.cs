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
					var t = line.Split(new[] { "kinopoisk_id|" }, StringSplitOptions.RemoveEmptyEntries);
					if (t.Length > 2)
					{
						var a = 10;
					}
					kinopoiskId = new string(t[1].TakeWhile(char.IsDigit).ToArray());
				}
				dataGridView1.Rows.Add(film, kinopoiskId);
			}

			Task.Run(() => LoadIds());
		}

		private void LoadIds()
		{
			var constants = new AppConstants();
			using (var br = new Browser(constants))
			{
				foreach (DataGridViewRow dataGridViewRow in dataGridView1.Rows)
				{
					if (dataGridViewRow.Cells[1].Value.IsNullOrEmpty())
					{
						var name = dataGridViewRow.Cells[0].Value.ToString();
						if (IsInCache(name))
						{
							dataGridViewRow.Cells[2].Value = GetFromCache(name);
						}
						else
						{
							br.FindFilm(name);
							var film = br.GetOpenedFilm(false);
							dataGridViewRow.Cells[2].Value = film.KinopoiskId;
						}
					}
					else
					{
						dataGridViewRow.Cells[2].Value = dataGridViewRow.Cells[1].Value;
					}
				}

			}
		}

		private bool IsInCache(string name)
		{
			var t = _cache.SingleOrDefault(p => Regex.Match(p, @"(.+)\s(\d+)$").Groups[1].Value.Trim() == name);
			return t != null;
		}

		private string GetFromCache(string name)
		{
			var t = _cache.Single(p => Regex.Match(p, @"(.+)\s(\d+)$").Groups[1].Value.Trim() == name);
			return new string(t.Trim().Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
		}

		private readonly string[] _cache = new[]
		{
			"Скуби-Ду (Scooby-Doo)		5091",
			"Женщина в черном (The Woman in Black)		498707",
			"Секс по дружбе (Friends with Benefits)		505811",
			"Старик (Shal)		711443",
			"Железный человек 2 (Iron Man 2)		411924",
			"Король шаманов (Shaman King)		280155",
			"Тор (Thor)		258941",
			"Железный человек (Iron Man)		61237",
			"Женщина-кошка (Catwoman)		3523",
			@"Дневник Бриджет Джонс (Bridget Jones\\\'s Diary)		621",
			"Клятва  (The Vow)		506296",
			"Мстители (The Avengers)		263531",
			"Держи ритм (Take the Lead)		102147",
			"Один дома (Home Alone)		8124",
			"Солт (Salt)		447284",
			"Паранормальное явление (Paranormal Activity)		404366",
			"Воображариум (Imaginaerum)		609627",
			"Помни меня (Remember Me)		461782",
			"Кит (Keith)		196855",
			"Виолет и Дейзи (Violet & Daisy)		507984",
			"Околофутбола		705162",
			"Идеальный голос (Pitch Perfect)		596251",
			"Капитан Филлипс (Captain Phillips)		484438",
			"Шерлок Холмс (Sherlock Holmes)		420923",
			"Евротур (EuroTrip)		5090",
			"Подземелье драконов (Dungeons & Dragons)		11109",
			"Dungeons & Dragons: Wrath of the Dragon God		81698",
			"Dungeons & Dragons: The Book of Vile Darkness		568351",
			"Мисс Конгениальность (Miss Congeniality)		755",
			"Miss Congeniality 2: Armed & Fabulous		71341",
			"Сверхъестественное 6 сезон (Supernatural)		178707",
			"Железный человек 3 (Iron Man Three)		462762",
			"Home Alone 2: Lost in New York		5928",
			"Служебный роман		43869",
			"Служебный роман. Наше время		468193",
			"Остров		81291",
			@"One Flew Over the Cuckoo\\\'s Nest		336",
			"Город бога (City of God)		439",
			"300 спартанцев		81924",
			"9 рота		84674",
			"Мы из будущего		260315",
			"Мы из будущего 2		468123",
			"Говори (Speak)		61628",
			"Форсаж (The Fast and the Furious)		666",
			"Форсаж 2 (The Fast and the Furious 2)		323",
			"Тройной форсаж: Токийский Дрифт		106079",
			"Форсаж 4 (The Fast and the Furious 4)		395978",
			"Малефисента (Maleficent)		496849",
			"Форсаж 5 (Fast Five)		496943",
			"Форсаж 6 (Furious 6)		594736",
			"Приглашение на развод (Divorce Invitation)		603324",
			"Кадры (The Internship)		666935",
			"Вторжение титанов  (Shingeki no Kyojin)		749374",
			"Волк с Уолл-стрит (The Wolf of Wall Street)		462682",
			"Конец света 2013: Апокалипсис по-голливудски (This Is the End)		462514",
			"Ван Хельсинг (Van Helsing)		3491",
			"ВАЛЛ·И (WALL·E)		279102",
			"Петля времени (Looper)		463354",
			"Сумерки (Twilight)		401177",
			"Сумерки. Сага. Новолуние		420224",
			"Сумерки. Сага. Затмение		432423",
			"Элизиум: Рай не на Земле (Elysium)		487409",
			"The Twilight Saga: Breaking Dawn - Part 1		432424",
			"Ронал-варвар (Ronal Barbaren)		549198",
			"Шутки в сторону		669982",
			"1+1 (Intouchables)		535341",
			"Голодные игры (The Hunger Games)		468581",
			"Последние дни на Марсе (The Last Days on Mars)		566111",
			"Голодные игры: И вспыхнет пламя (The Hunger Games: Catching Fire)		602373",
			"Скуби-Ду 2: Монстры на свободе (Scooby Doo 2: Monsters Unleashed)		6186",
			"Скуби-Ду 3- Тайна начинается (Scooby-Doo! The Mystery Begins)		419046",
			"Нежность (La dеlicatesse)		582764",
			@"Амели (Le Fabuleux destin d\\\'Am?lie Poulain)		341",
			"Дорогой Джон (Dear John)		415687",
			"Каникулы строгого режима		417840",
			"Легенда №17		601564",
			"Перси Джексон и Море чудовищ (Percy Jackson: Sea of Monsters)		581932",
			"Перси Джексон и похититель молний		403986",
			"Письма к Джульетте (Letters to Juliet)		461799",
			"Синдбад: Легенда семи морей (Sinbad: Legend of the Seven Seas)		7971",
			"Мачете убивает (Machete Kills)		601022",
			"Мачете (Machete)		467553",
			"Великий Гэтсби (The Great Gatsby)		463724",
			"Бэтмен: Начало (Batman Begins)		47237",
			"Темный рыцарь: Возрождение легенды (The Dark Knight Rises)		437410",
			"Темный рыцарь (The Dark Knight)		111543",
			"Милый друг (Bel Ami)		466849",
			"Тайный знак (An Invisible Sign)		410566",
			"Несносный дед (Jackass Presents: Bad Grandpa)		780451",
			"Начало (Inception)		447301",
			"Тепло наших тел (Warm Bodies)		497077",
			"Сверхъестественное 1 сезон (Supernatural)		178707",
			"Сверхъестественное 2 сезон (Supernatural)		178707",
			"Восьмидесятые (1-й сезон)		592383",
			"Сверхъестественное 3 сезон (Supernatural)		178707",
			"Сверхъестественное 4 сезон (Supernatural)		178707",
			"Сверхъестественное 5 сезон (Supernatural)		178707",
			"Гриффины (1 сезон)		161101",
			"Гриффины (2 сезон)		161101",
			"Гриффины (3 сезон)		161101",
			"Гриффины (4 сезон)		161101",

		};
	}

	public static class ExtensionMethods
	{
		public static bool IsNullOrEmpty(this object obj)
		{
			if (obj == null)
			{
				return true;
			}

			return string.IsNullOrEmpty(obj.ToString());
		}
	}
}
