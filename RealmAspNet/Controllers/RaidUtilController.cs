using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Colourful;
using Microsoft.Extensions.Logging;
using RealmAspNet.Definitions;
using RealmAspNet.Models;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Player;
using Tesseract;

namespace RealmAspNet.Controllers
{
	[Route("api/raidutil")]
	[ApiController]
	public class RaidUtilController : ControllerBase
	{
		private long _jobCount;
		private readonly ILogger<RaidUtilController> _logger;
		private readonly TesseractEngine _tesseractEngine;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RaidUtilController(ILogger<RaidUtilController> logger)
		{
			_jobCount = 0;
			_logger = logger;
			_tesseractEngine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
		}

		/// <summary>
		/// Parses a who screenshot.
		/// </summary>
		/// <param name="model">An object containing the URL.</param>
		/// <returns>The parse results.</returns>
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
				await using var resp = await Constants.BaseClient.GetStreamAsync(uri);
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
			
			sw.Stop();
			
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
		
		/// <summary>
		/// Gets all the player's basic information (the player's RealmEye "homepage").
		/// </summary>
		/// <param name="names">The names.</param>
		/// <returns>The response.</returns>
		[HttpPost("parse")]
		public async Task<IActionResult> DoParseJob([FromBody] string[] names)
		{
			var jobId = _jobCount++;
			_logger.LogInformation($"[DoParseJob] Started Job {jobId}. Name Count: {names.Length}");
			var stopwatch = Stopwatch.StartNew();
			
			var job = new ParseJob
			{
				JobId = jobId,
				Elapsed = 0,
				CompletedCount = 0,
				FailedCount = 0,
				Finished = false,
				Output = new List<PlayerData>(),
				Completed = new List<string>(),
				Failed = new List<string>(),
				Input = names.ToList()
			};
			
			var profiles = await Task.WhenAll(names.Select(PlayerScraper.ScrapePlayerProfileAsync).ToList());
			foreach (var profile in profiles)
			{
				if (profile.ResultCode is not ResultCode.Success)
				{
					job.Failed.Add(profile.Name);
					continue; 
				}
				
				job.Completed.Add(profile.Name);
				job.Output.Add(profile);
			}

			stopwatch.Stop();
			job.Elapsed = stopwatch.Elapsed.TotalSeconds;
			job.CompletedCount = job.Completed.Count;
			job.FailedCount = job.Failed.Count;
			_logger.LogInformation($"[DoParseJob] Finished Job {jobId}. Time: {job.Elapsed} Seconds.\n" 
			                       + $"\t- Completed: {job.CompletedCount}\n" 
			                       + $"\t- Failed: {job.FailedCount} ({string.Join(", ", job.Failed)})");
			return Ok(job);
		}
	}

	class ParseJob
	{
		public long JobId { get; set; }
		public double Elapsed { get; set; }
		public bool Finished { get; set; }
		public int CompletedCount { get; set; }
		public int FailedCount { get; set; }
		public List<string> Input { get; set; }
		public List<string> Completed { get; set; }
		public List<string> Failed { get; set; }
		public List<PlayerData> Output { get; set; }
	}
}