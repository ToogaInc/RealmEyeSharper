using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealmAspNet.Definitions;
using RealmAspNet.Models;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Player;

// using Tesseract;

namespace RealmAspNet.Controllers
{
    [Route("api/raidutil")]
    [ApiController]
    public class RaidUtilController : ControllerBase
    {
        private readonly Dictionary<int, ParseJob> _currentParseJobs;
        private readonly ILogger<RaidUtilController> _logger;

        private int _idx;
        // private readonly TesseractEngine _tesseractEngine;

        /// <summary>
        ///     Creates a new controller for this API.
        /// </summary>
        /// <param name="logger">The logging object.</param>
        public RaidUtilController(ILogger<RaidUtilController> logger)
        {
            _idx = 0;
            _logger = logger;
            // _tesseractEngine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            _currentParseJobs = new Dictionary<int, ParseJob>();
        }

// 		/// <summary>
// 		/// Parses a who screenshot.
// 		/// </summary>
// 		/// <param name="model">An object containing the URL.</param>
// 		/// <returns>The parse results.</returns>
// 		[HttpGet("parsewho")]
// 		public async Task<ParseWhoResult> ParseWhoScreenshot([FromBody] ParseWhoModel model)
// 		{
// 			var url = HttpUtility.UrlDecode(model.Url);
// 			var uri = Uri.TryCreate(url, UriKind.Absolute, out var uriRes)
// 			          && (uriRes.Scheme == Uri.UriSchemeHttp || uriRes.Scheme == Uri.UriSchemeHttps)
// 				? uriRes
// 				: null;

// 			var logStr = new StringBuilder()
// 				.Append($"[{DateTime.Now:G}] For URL Input: {url}");

// 			var returnObj = new ParseWhoResult
// 			{
// 				Count = 0,
// 				ImageDownloadTime = 0,
// 				ImageProcessingTime = 0,
// 				OcrRecognitionTime = 0,
// 				RawOcrResult = string.Empty,
// 				WhoResult = Array.Empty<string>(),
// 				Issues = string.Empty,
// 				Code = string.Empty
// 			};

// 			if (uri == null)
// 			{
// 				logStr.AppendLine().Append("\tUnable to parse URL.");
// 				_logger.LogInformation(logStr.ToString());
// 				returnObj.Issues = "The given URL could not be parsed.";
// 				returnObj.Code = "FAILED:INVALID_URL";
// 				return returnObj;
// 			}

// 			Bitmap image;
// 			var sw = new Stopwatch();
// 			sw.Start();

// 			// get image
// 			try
// 			{
// 				await using var resp = await Constants.BaseClient.GetStreamAsync(uri);
// 				image = new Bitmap(resp);
// 			}
// 			catch (Exception)
// 			{
// 				logStr.AppendLine().Append("\tURL points to invalid location.");
// 				_logger.LogInformation(logStr.ToString());
// 				returnObj.Issues = "The given URL pointed to an invalid location (404 error, perhaps).";
// 				returnObj.Code = "FAILED:URL_INVALID_LOCATION";
// 				return returnObj;
// 			}

// 			sw.Stop();

// 			returnObj.ImageDownloadTime = sw.ElapsedMilliseconds;
// 			logStr.AppendLine().Append($"\tImage Download Time: {returnObj.ImageDownloadTime} MS");

// 			sw.Restart();

// 			// this is a slight shade of yellow but divide each value by 255
// 			var baseYellow = new RGBColor(0.8928976034858388, 0.9003921568627451, 0.04435729847494554);
// 			var converter = new ConverterBuilder()
// 				.FromRGB()
// 				.ToLab()
// 				.Build();
// 			for (var x = 0; x < image.Width; ++x)
// 			{
// 				for (var y = 0; y < image.Height; ++y)
// 				{
// 					var pixel = image.GetPixel(x, y);
// 					var rgbColor = new RGBColor(pixel.R / 255.0, pixel.G / 255.0, pixel.B / 255.0);
// 					var labColorOfBaseYellow = converter.Convert(baseYellow);
// 					var labColorOfPixel = converter.Convert(rgbColor);
// 					var deltaE = new CIEDE2000ColorDifference()
// 						.ComputeDifference(labColorOfBaseYellow, labColorOfPixel);

// 					image.SetPixel(x, y, deltaE < 15 ? Color.Black : Color.White);
// 				}
// 			}

// 			sw.Stop();
// 			returnObj.ImageProcessingTime = sw.ElapsedMilliseconds;

// #if DEBUG
// 			image.Save(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.png"));
// #endif

// 			sw.Restart();
// 			using var page = _tesseractEngine.Process(image);
// 			sw.Stop();

// 			returnObj.OcrRecognitionTime = sw.ElapsedMilliseconds;
// 			returnObj.RawOcrResult = page.GetText();

// 			logStr.AppendLine().Append($"\tImage Processing Time: {returnObj.ImageProcessingTime} MS.")
// 				.AppendLine().Append($"\tOCR Time: {returnObj.OcrRecognitionTime} MS.")
// 				.AppendLine().Append($"\tRaw OCR Results: {returnObj.RawOcrResult.Replace("\n", " <NL> ")}");


// 			var textArr = page.GetText().Split("\n")
// 				.Select(x => x.Trim())
// 				.ToArray();

// 			var index = -1;
// 			for (var i = 0; i < textArr.Length; ++i)
// 			{
// 				if (!textArr[i].ToLower().StartsWith("players online")
// 				    || !textArr[i].ToLower().Contains("):"))
// 					continue;
// 				index = i;
// 				break;
// 			}

// 			if (index == -1)
// 			{
// 				logStr.AppendLine().Append("\tUnable to find \"player online\" or \"):\" in text.");
// 				_logger.LogInformation(logStr.ToString());
// 				returnObj.Issues = "Unable to find \"player online\" or \"):\" in text.";
// 				returnObj.Code = "FAILED:NO_WHO_TEXT_FOUND";
// 				return returnObj;
// 			}

// 			var nameArr = new HashSet<string>();
// 			var firstLine = textArr[index].Split("):")[1].Trim();
// 			var peopleInFirstLine = firstLine.Split('.', ',')
// 				.Select(x => x.Trim())
// 				.Where(x => x != string.Empty)
// 				.ToArray();

// 			foreach (var name in peopleInFirstLine)
// 				nameArr.Add(name.Replace('0', 'O').Replace('1', 'I'));

// 			var emptyLineSuccession = 0;
// 			for (var i = index + 1; i < textArr.Length; ++i)
// 			{
// 				if (string.IsNullOrEmpty(textArr[i])
// 				    || !char.IsLetter(textArr[i][0]))
// 				{
// 					++emptyLineSuccession;
// 					continue;
// 				}

// 				if (emptyLineSuccession >= 2)
// 					break;

// 				var peopleInThisLine = textArr[i].Split('.', ',')
// 					.Select(x => x.Trim())
// 					.Where(x => x != string.Empty)
// 					.ToArray();

// 				if (peopleInThisLine.Length == 0)
// 				{
// 					++emptyLineSuccession;
// 					continue;
// 				}

// 				emptyLineSuccession = 0;

// 				foreach (var name in peopleInThisLine)
// 					nameArr.Add(name.Replace('0', 'O').Replace('1', 'I'));
// 			}

// 			returnObj.Count = nameArr.Count;
// 			returnObj.WhoResult = nameArr.ToArray();
// 			returnObj.Code = "SUCCESS";
// 			logStr.AppendLine().Append($"\tParse Successful! {returnObj.Count} names parsed.")
// 				.AppendLine().Append($"\tNames parsed: {string.Join(", ", returnObj.WhoResult)}");
// 			_logger.LogInformation(logStr.ToString());
// 			return returnObj;
// 		}

        [HttpGet("parse/img/")]
        public async Task<IActionResult> ParseImage([FromBody] ParseImgModel model)
        {
#if DEBUG
            Console.WriteLine("[ParseImg] Sending image to OCR endpoint.");
#endif
            
            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("url", model.Url),
                new("isOverlayRequired", "true"),
                new("scale", "false"),
                new("OCREngine", "2")
            });
            using var reqRes = await Constants.OCRClient.PostAsync("https://api.ocr.space/parse/image", formContent);

#if DEBUG
            Console.WriteLine(await reqRes.Content.ReadAsStringAsync());
#endif

            var json = JsonConvert.DeserializeObject<OCRResult>(await reqRes.Content.ReadAsStringAsync());
            if (json is null || json.OcrExitCode != 1) return StatusCode(500);

            var parsePlayers = new Func<string, List<string>>(str =>
            {
                if (str.Contains(":")) str = str.Substring(str.IndexOf(":", StringComparison.Ordinal) + 1);

                var split = str.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                for (var i = 0; i < split.Count; i++) split[i] = split[i].Replace(" ", "").Replace("0", "o");
#if DEBUG
                Console.WriteLine("[Printing players]: " +
                                  string.Join(", ", split));
#endif

                return split;
            });
            
            
            double left = -1;
            double top = -1;
            var players = new List<string>();

            foreach (var line in json.ParsedResults[0].TextOverlay.Lines)
            {
                var firstWord = line.Words[0];

                if (line.LineText.Contains("Players Online", StringComparison.OrdinalIgnoreCase))
                {
                    if (left != -1) players.Clear();
                    
                    left = firstWord.Left;
                    top = firstWord.Top;
                    players.AddRange(parsePlayers(line.LineText));
#if DEBUG
                    Console.WriteLine("Setting Bounds.. | Left: " + left + " | Top: " + top);
#endif
                }
                else if (left != -1 &&
                         left - 4 < firstWord.Left && firstWord.Left < left + 4 &&
                         firstWord.Top >= top)
                {
#if DEBUG
                    Console.WriteLine("Testing Line: " + line.LineText);
#endif
                    if (Regex.IsMatch(line.LineText, "^[a-zA-Z, 0]+"))
                        players.AddRange(parsePlayers(line.LineText));
                }
            }

            if (players.Count == 0) return Problem("No players detected!");
            
            return await StartParseJob(players.ToArray());
        }


        [HttpPost("parse")]
        public async Task<IActionResult> StartParseJob([FromBody] string[] model)
        {
            _logger.LogInformation($"[StartParseJob] Received Input: {string.Join(", ", model)}");
            var id = _idx++;
            _currentParseJobs.Add(id, new ParseJob
            {
                Finished = false,
                Output = new List<PlayerData>(),
                Completed = new List<string>(),
                Failed = new List<string>(),
                Input = model.ToList()
            });

            var nameQuery = model.Select(PlayerScraper.ScrapePlayerProfileAsync).ToList();
            while (nameQuery.Any())
            {
                var finishedTask = await Task.WhenAny(nameQuery);
                nameQuery.Remove(finishedTask);
                var result = await finishedTask;

                if (result.ResultCode != ResultCode.Success)
                {
                    _currentParseJobs[id].Failed.Add(result.Name);
                }
                else
                {
                    _currentParseJobs[id].Output.Add(result);
                    _currentParseJobs[id].Completed.Add(result.Name);
                }
            }

            return Ok(new {id});
        }

        [HttpGet("parse/{id}")]
        public IActionResult GetParseStatus(int id)
        {
            _logger.LogInformation($"[GetParseStatus] Received ID: {id}");
            if (_currentParseJobs.TryGetValue(id, out var job))
            {
                if (job.Input.Count == job.Completed.Count + job.Failed.Count)
                {
                    job.Finished = true;
                    _currentParseJobs.Remove(id);
                    _logger.LogInformation("\tJob Completed.");
                    return Ok(job);
                }

                _logger.LogInformation("\tNot Completed.");
                return Ok(new
                {
                    _currentParseJobs[id].Completed,
                    _currentParseJobs[id].Failed,
                    _currentParseJobs[id].Finished,
                    _currentParseJobs[id].Input,
                    Output = new List<int>()
                });
            }

            _logger.LogInformation("\tNot Found.");
            return NotFound();
        }
    }

    internal class ParseJob
    {
        public bool Finished { get; set; }
        public List<string> Input { get; set; }
        public List<string> Completed { get; set; }
        public List<string> Failed { get; set; }

        public List<PlayerData> Output { get; set; }
    }
}