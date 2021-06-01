using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Colourful;
using Microsoft.Extensions.Logging;
using RealmAspNet.Definitions;
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

		[HttpGet("parsewho")]
		public async Task<ParseWhoResult> ParseWhoScreenshot([FromBody] ParseWhoModel model)
		{
			var url = HttpUtility.UrlDecode(model.Url);
			var uri = Uri.TryCreate(url, UriKind.Absolute, out var uriRes)
			          && (uriRes.Scheme == Uri.UriSchemeHttp || uriRes.Scheme == Uri.UriSchemeHttps)
				? uriRes
				: null;

			var logStr = new StringBuilder()
				.Append($"[{DateTime.Now:G}] For URL Input: {url}");

			var returnObj = new ParseWhoResult
			{
				Count = 0,
				ImageDownloadTime = 0,
				ImageProcessingTime = 0,
				OcrRecognitionTime = 0,
				RawOcrResult = string.Empty,
				WhoResult = Array.Empty<string>(),
				Issues = string.Empty,
				Code = string.Empty
			};

			if (uri == null)
			{
				logStr.AppendLine().Append("\tUnable to parse URL.");
				_logger.LogInformation(logStr.ToString());
				returnObj.Issues = "The given URL could not be parsed.";
				returnObj.Code = "FAILED:INVALID_URL";
				return returnObj;
			}

			Bitmap image;
			var sw = new Stopwatch();
			sw.Start();

			// get image
			try
			{
				await using var resp = await _client.GetStreamAsync(uri);
				image = new Bitmap(resp);
			}
			catch (Exception)
			{
				logStr.AppendLine().Append("\tURL points to invalid location.");
				_logger.LogInformation(logStr.ToString());
				returnObj.Issues = "The given URL pointed to an invalid location (404 error, perhaps).";
				returnObj.Code = "FAILED:URL_INVALID_LOCATION";
				return returnObj;
			}

			returnObj.ImageDownloadTime = sw.ElapsedMilliseconds;
			logStr.AppendLine().Append($"\tImage Download Time: {returnObj.ImageDownloadTime} MS");

			sw.Restart();

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

			sw.Stop();
			returnObj.ImageProcessingTime = sw.ElapsedMilliseconds;

#if DEBUG
			image.Save(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.png"));
#endif

			sw.Restart();
			using var page = _tesseractEngine.Process(image);
			sw.Stop();

			returnObj.OcrRecognitionTime = sw.ElapsedMilliseconds;
			returnObj.RawOcrResult = page.GetText();

			logStr.AppendLine().Append($"\tImage Processing Time: {returnObj.ImageProcessingTime} MS.")
				.AppendLine().Append($"\tOCR Time: {returnObj.OcrRecognitionTime} MS.")
				.AppendLine().Append($"\tRaw OCR Results: {returnObj.RawOcrResult.Replace("\n", " <NL> ")}");


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
			{
				logStr.AppendLine().Append("\tUnable to find \"player online\" or \"):\" in text.");
				_logger.LogInformation(logStr.ToString());
				returnObj.Issues = "Unable to find \"player online\" or \"):\" in text.";
				returnObj.Code = "FAILED:NO_WHO_TEXT_FOUND";
				return returnObj;
			}

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

			returnObj.Count = nameArr.Count;
			returnObj.WhoResult = nameArr.ToArray();
			returnObj.Code = "SUCCESS";
			logStr.AppendLine().Append($"\tParse Successful! {returnObj.Count} names parsed.")
				.AppendLine().Append($"\tNames parsed: {string.Join(", ", returnObj.WhoResult)}");
			_logger.LogInformation(logStr.ToString());
			return returnObj;
		}
	}
}