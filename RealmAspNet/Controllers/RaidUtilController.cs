using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Colourful;
using Microsoft.Extensions.Logging;
using RealmAspNet.Models;
using Tesseract;

namespace RealmAspNet.Controllers
{
	[Route("api/raidutil")]
	[ApiController]
	public class RaidUtilController : ControllerBase
	{
		private readonly ILogger<RaidUtilController> _logger;
		private readonly TesseractEngine _tesseractEngine;
		private readonly HttpClient _client; 

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RaidUtilController(ILogger<RaidUtilController> logger)
		{
			_logger = logger;
			_tesseractEngine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
			_client = new HttpClient();
		}

		public struct ParseBody
		{
			public string Url { get; set; }
		}
		
		[HttpGet("parsewho")]
		public async Task<string[]> ParseWhoScreenshot([FromBody] ParseWhoModel model)
		{
			var url = HttpUtility.UrlDecode(model.Url);
			_logger.LogInformation($"ParseWho Executed. URL: {url}");
			var uri = Uri.TryCreate(url, UriKind.Absolute, out var uriRes)
			          && (uriRes.Scheme == Uri.UriSchemeHttp || uriRes.Scheme == Uri.UriSchemeHttps)
				? uriRes
				: null;

			if (uri == null)
				return Array.Empty<string>();

			Bitmap image;

			// get image
			try
			{
				await using var resp = await _client.GetStreamAsync(uri);
				image = new Bitmap(resp);
			}
			catch (Exception)
			{
				return Array.Empty<string>();
			}


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

					image.SetPixel(x, y, deltaE < 15 ? Color.Black : Color.White);
				}
			}

#if DEBUG
			image.Save(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.png"));
#endif
			
			using var page = _tesseractEngine.Process(image);
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
				return Array.Empty<string>();

			var nameArr = new HashSet<string>();
			var firstLine = textArr[index].Split("):")[1].Trim();
			var peopleInFirstLine = firstLine.Split('.', ',')
				.Select(x => x.Trim())
				.Where(x => x != string.Empty)
				.ToArray();

			foreach (var name in peopleInFirstLine)
				nameArr.Add(name.Replace('0', 'O').Replace('1', 'I'));

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
					nameArr.Add(name.Replace('0', 'O').Replace('1', 'I'));
			}

			return nameArr.ToArray();
		}
	}
}