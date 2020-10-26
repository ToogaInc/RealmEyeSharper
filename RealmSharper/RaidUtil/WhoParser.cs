using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Colourful;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace RealmSharper.RaidUtil
{
	public static class WhoParser
	{
		private static readonly HttpClient Client = new HttpClient();

		public static async Task<string[]> ParseWhoScreenshot(WhoInput input)
		{
			Bitmap image = null;
			if (input.Url != null)
			{
				await using var resp = await Client.GetStreamAsync(input.Url);
				image = new Bitmap(resp);
			}
			else if (input.Base64 != null)
			{
				var byteData = Convert.FromBase64String(input.Base64);
				await using var stream = new MemoryStream(byteData);
				image = new Bitmap(stream);
			}

			if (image == null)
				return new string[0];

			// this is a slight shade of yellow but divide each value by 255
			var baseYellow = new RGBColor(0.8928976034858388, 0.9003921568627451, 0.04435729847494554);
			var converter = new ConverterBuilder()
				.FromRGB()
				.ToLab()
				.Build();
			for (var x = 0; x < image.Width; ++x)
			{
				for (var y = 0; y < image.Height; ++y)
				{
					var pixel = image.GetPixel(x, y);
					var rgbColor = new RGBColor(pixel.R / 255.0, pixel.G / 255.0, pixel.B / 255.0);
					var labColorOfBaseYellow = converter.Convert(baseYellow);
					var labColorOfPixel = converter.Convert(rgbColor);
					var deltaE = new CIEDE2000ColorDifference()
						.ComputeDifference(labColorOfBaseYellow, labColorOfPixel);

					image.SetPixel(x, y, deltaE < 10 ? Color.Black : Color.White);
				}
			}

			await using var anotherStream = new MemoryStream();
			image.Save(anotherStream, ImageFormat.Png);
			image.Save(@"C:\Users\ewang\Desktop\bigTest.png", ImageFormat.Png);

			using var engine = new TesseractEngine(Path.Join(".", "tessdata"), "eng", EngineMode.Default);
			using var page = engine.Process(Pix.LoadFromMemory(anotherStream.ToArray()));
			var textArr = page.GetText().Split("\n")
				.Select(x => x.Trim())
				.ToArray();

			var index = -1; 
			for (var i = 0; i < textArr.Length; ++i)
			{
				if (!textArr[i].ToLower().StartsWith("players online") 
				    || !textArr[i].ToLower().Contains("):")) 
					continue;
				index = i;
				break;
			}

			if (index == -1)
				return new string[0];

			var nameArr = new HashSet<string>();
			var firstLine = textArr[index].Split("):")[1].Trim();
			var peopleInFirstLine = firstLine.Split('.', ',')
				.Select(x => x.Trim())
				.Where(x => x != string.Empty)
				.ToArray();

			foreach (var name in peopleInFirstLine)
				nameArr.Add(name);

			var emptyLineSuccession = 0;
			for (var i = index + 1; i < textArr.Length; ++i)
			{
				if (string.IsNullOrEmpty(textArr[i])
				    || !char.IsLetter(textArr[i][0]))
				{
					++emptyLineSuccession;
					continue;
				}
				
				if (emptyLineSuccession >= 2)
					break;

				var peopleInThisLine = textArr[i].Split('.', ',')
					.Select(x => x.Trim())
					.Where(x => x != string.Empty)
					.ToArray();

				if (peopleInThisLine.Length == 0)
				{
					++emptyLineSuccession;
					continue;
				}

				emptyLineSuccession = 0;

				foreach (var name in peopleInThisLine)
					nameArr.Add(name);
			}

			return nameArr.ToArray();
		}
	}

	public struct WhoInput
	{
		public string Url { get; set; }
		public string Base64 { get; set; }
	}
}