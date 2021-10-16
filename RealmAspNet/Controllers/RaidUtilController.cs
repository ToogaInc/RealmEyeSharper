// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable SuggestBaseTypeForParameter

#define NO_PRINT
#define USE_ACTION_BLOCK
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealmAspNet.Definitions;
using RealmAspNet.Models;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Player;

namespace RealmAspNet.Controllers
{
	[Route("api/raidutil")]
	[ApiController]
	public class RaidUtilController : ControllerBase
	{
		private static readonly string[] DefaultNames =
		{
			"darq", "deyst", "drac", "drol", "eango", "eashy", "eati", "eendi", "ehoni", "gharr", "iatho", "iawa",
			"idrae", "iri", "issz", "itani", "laen", "lauk", "lorz", "oalei", "odaru", "oeti", "orothi", "oshyu",
			"queq", "radph", "rayr", "ril", "rilr", "risrr", "saylt", "scheev", "sek", "serl", "seus", "tal",
			"tiar", "uoro", "urake", "utanu", "vorck", "vorv", "yangu", "yimi", "zhiar"
		};

		private readonly ILogger<RaidUtilController> _logger;
		private int _apiReqJobId;
		private int _concurrJobId;

		/// <summary>
		///     Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RaidUtilController(ILogger<RaidUtilController> logger)
		{
			_concurrJobId = 0;
			_apiReqJobId = 0;
			_logger = logger;
		}


		/// <summary>
		/// Parses a /who screenshot. Uses OCR.space's OCR API to get text from a /who screenshot.
		/// </summary>
		/// <param name="model">The model. This should contain an URL.</param>
		/// <returns>The parse results.</returns>
		[HttpPost("parseWhoOnly")]
		public async Task<IActionResult> ParseWhoScreenshotAsync([FromBody] WhoParseOnlyModel model)
		{
			if (!Constants.UseOcr)
				return Problem("No OCR.Space API Key", null, 503);
			
			var jobId = _apiReqJobId++;

			// To make it clearer that there's a new method being called.
			Console.WriteLine("\n");
			_logger.LogInformation(
				$"[ParseImgWhoOnly] [ID {jobId}] Received Image URL for Parsing. URL: {model.Url}\n"
				+ $"\t- Scaling: {model.Scale}"
			);

			var stopwatch = Stopwatch.StartNew();
#if DEBUG && !NO_PRINT
			Console.WriteLine("[ParseImgWhoOnly] Sending Image to OCR Endpoint.");
#endif

			// C# "true.toString()" returns "True" not "true"
			var names = await ProcessImg(model.Url, model.Scale ? "true" : "false");

			stopwatch.Stop();

			// If we're only interested in basic /who parsing, then run this and return.
			_logger.LogInformation(
				$"[ParseImgWhoOnly] [ID {jobId}] /who Parsing Successful.\n"
				+ $"\t- Count: {names.Count}\n"
				+ $"\t- Time: {stopwatch.Elapsed.TotalSeconds} Seconds."
			);

			return Ok(new {Names = names, TimeElapsedSec = stopwatch.Elapsed.TotalSeconds});
		}

		/// <summary>
		/// Parses a /who screenshot and gets all the player's basic information (the player's RealmEye "homepage").
		/// Uses OCR.space's OCR API to get text from a /who screenshot.
		/// </summary>
		/// <param name="model">The model. This should contain an URL.</param>
		/// <returns>The parse results.</returns>
		[HttpPost("parseWhoRealmEye")]
		public async Task<IActionResult> ParseWhoScreenshotAndGetDataAsync([FromBody] ParseImgModel model)
		{
			if (!Constants.UseOcr)
				return Problem("No OCR.Space API Key", null, 503);
			if (!Constants.UseProxy)
				return Problem("No Webshare.io API Key", null, 503);
			
			var jobId = _apiReqJobId++;
			// To make it clearer that there's a new method being called.
			Console.WriteLine("\n");
			_logger.LogInformation(
				$"[ParseImgGetData] [ID {jobId}] Received Image URL for Parsing.\n"
				+ $"\t- URL: {model.Url}\n"
				+ $"\t- Simple Parse?: {model.SimpleParse ?? false}"
			);

			var stopwatch = Stopwatch.StartNew();
#if DEBUG && !NO_PRINT
			Console.WriteLine("[ParseImg] Sending Image to OCR Endpoint.");
#endif

			var unscaledTask = ProcessImg(model.Url, "false");

			// If we're only interested in basic /who parsing and RE data, then run this and return.
			if (model.SimpleParse ?? false)
			{
				var names = await unscaledTask;
				if (names.Count == 0)
					return Problem("No players detected!");

				var parseTimeTaken = stopwatch.Elapsed.TotalSeconds;
				var reReq = await SendConcurrentRealmEyeRequestsAsync(names.ToArray());

				reReq.ParseWhoElapsedSec = parseTimeTaken;
				reReq.TotalElapsedSec = reReq.ConcurrElapsedSec + parseTimeTaken;
				_logger.LogInformation(
					$"[ParseImgGetData:Simple] [ID {jobId}] /who Parsing + RE Data Request Successful.\n"
					+ $"\t- Total Time: {reReq.TotalElapsedSec} Seconds.\n"
					+ $"\t- Parse Who Time: {parseTimeTaken} Seconds.\n"
					+ $"\t- Concurr. Req. Time: {reReq.ConcurrElapsedSec} Seconds."
					+ $"\t- Completed: {reReq.CompletedCount}\n"
					+ $"\t- Failed: {reReq.FailedCount}"
				);
				return Ok(reReq);
			}

			// Otherwise, we're going to do more advanced /who parsing and get RE data.
			var scaledTask = ProcessImg(model.Url, "true");
			var allNames = await Task.WhenAll(unscaledTask, scaledTask);

			var ocrTimeTaken = stopwatch.Elapsed.TotalSeconds;
			_logger.LogInformation(
				$"[ParseImgGetData] [ID {jobId}] OCR Parse Completed in {ocrTimeTaken} Seconds."
			);

			var unscaledPlayers = allNames[0];
			var scaledPlayers = allNames[1];

#if DEBUG && !NO_PRINT
			Console.WriteLine("[ParseImg:Players] Player List (Common): " + 
			                  String.Join(", ", unscaledPlayers));
			Console.WriteLine("[ParseImg:Players] Player List (Diff.): " + 
			                  String.Join(", ", scaledPlayers));
#endif

			var unscaledRes = await SendConcurrentRealmEyeRequestsAsync(unscaledPlayers.ToArray());
			Dictionary<string, string> fixedNames = new();
			// Try to find the name in the unscaled.failed list that looks the closest in the scaled names.
			foreach (var name in unscaledRes.Failed)
			{
				var minDist = 5;
				string minName = null;
				foreach (var sName in scaledPlayers)
				{
					var dist = LevenshteinDistance.Compute(name, sName);
					if (dist < 3)
					{
						minName = sName;
						break;
					}

					if (dist < minDist)
					{
						minDist = dist;
						minName = sName;
					}
				}

				if (minName != null)
					fixedNames.Add(name, minName);
			}

			// Send requests for the fixed names.
			var fixedNamesRes = await SendConcurrentRealmEyeRequestsAsync(fixedNames.Values.ToArray());

			var newInput = unscaledRes.Input.ToList();
			foreach (var name in unscaledRes.Input)
				if (fixedNames.ContainsKey(name) && fixedNamesRes.Completed.Contains(fixedNames[name]))
				{
					if (unscaledRes.Failed.Contains(name))
						unscaledRes.Failed.Remove(name);
					newInput.Remove(name);
					newInput.Add(fixedNames[name]);
				}

			// Update the return object appropriately.
			unscaledRes.TotalElapsedSec += ocrTimeTaken + fixedNamesRes.ConcurrElapsedSec;
			unscaledRes.ConcurrElapsedSec += fixedNamesRes.ConcurrElapsedSec;
			unscaledRes.ParseWhoElapsedSec = ocrTimeTaken;
			unscaledRes.Input = newInput;
			unscaledRes.Completed.AddRange(fixedNamesRes.Completed);
			unscaledRes.CompletedCount = unscaledRes.Completed.Count;
			unscaledRes.FailedCount = unscaledRes.Failed.Count;
			unscaledRes.Output.AddRange(fixedNamesRes.Output);

			if (unscaledRes.CompletedCount == 0)
				return Problem("No players detected!");

			_logger.LogInformation(
				$"[ParseImgGetData] [ID {jobId}] /who Parsing + RE Data Request Successful.\n"
				+ $"\t- Total Time: {unscaledRes.TotalElapsedSec} Seconds.\n"
				+ $"\t- Parse Who Time: {unscaledRes.ParseWhoElapsedSec} Seconds.\n"
				+ $"\t- Concurr. Req. Time: {unscaledRes.ConcurrElapsedSec} Seconds.\n"
				+ $"\t- Completed: {unscaledRes.CompletedCount}\n"
				+ $"\t- Failed: {unscaledRes.FailedCount}"
			);

			return Ok(unscaledRes);
		}


		/// <summary>
		/// Gets all the player's basic information (the player's RealmEye "homepage").
		/// </summary>
		/// <param name="names">The names.</param>
		/// <returns>The response.</returns>
		[HttpPost("namesRealmEye")]
		public async Task<IActionResult> RequestMultipleRealmEyeProfilesAsync([FromBody] string[] names)
		{
			if (!Constants.UseProxy)
				return Problem("No Webshare.io API Key", null, 503);
			
			return Ok(await SendConcurrentRealmEyeRequestsAsync(names));
		}

		/// <summary>
		/// Sends multiple RealmEye requests.
		/// </summary>
		/// <param name="names">The names to get data for.</param>
		/// <returns>The job result.</returns>
		private async Task<ParseJob> SendConcurrentRealmEyeRequestsAsync(string[] names)
		{
			var jobId = _concurrJobId++;
			// To make it clearer that there's a new method being called.
			Console.WriteLine();
			_logger.LogInformation(
				$"[ConcurRealmEyeReq] [ID {jobId}] Started Job.\n"
				+ $"\t- Name Count: {names.Length}"
			);

			var stopwatch = Stopwatch.StartNew();

			var defaults = names.Intersect(DefaultNames, StringComparer.OrdinalIgnoreCase).ToList();
			names = names.Except(defaults).ToArray();

			var job = new ParseJob
			{
				TotalElapsedSec = 0,
				ParseWhoElapsedSec = 0,
				ConcurrElapsedSec = 0,
				CompletedCount = 0,
				FailedCount = 0,
				Output = new List<PlayerData>(),
				Completed = new List<string>(),
				Failed = new List<string>(),
				DefaultNames = defaults,
				Input = names.ToList()
			};

#if USE_ACTION_BLOCK
			// Essentially sending these requests concurrently with a limit.
			var bag = new ConcurrentBag<PlayerData>();
			var block = new ActionBlock<string>(
				async name => bag.Add(await PlayerScraper.ScrapePlayerProfileAsync(name)),
				new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 40}
			);

			foreach (var name in names) await block.SendAsync(name);

			block.Complete();
			await block.Completion;

			var profiles = bag.ToList();
#else
			var profiles = await Task.WhenAll(names.Select(PlayerScraper.ScrapePlayerProfileAsync));
#endif

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
			job.TotalElapsedSec = stopwatch.Elapsed.TotalSeconds;
			job.ConcurrElapsedSec = stopwatch.Elapsed.TotalSeconds;
			job.CompletedCount = job.Completed.Count;
			job.FailedCount = job.Failed.Count;

			_logger.LogInformation(
				$"[ConcurRealmEyeReq] [ID {jobId}] Finished Job.\n"
				+ $"\t- Time: {job.TotalElapsedSec} Seconds.\n"
				+ $"\t- Completed: {job.CompletedCount}\n"
				+ $"\t- Failed: {job.FailedCount} ({string.Join(", ", job.Failed)})"
			);
			return job;
		}

		/// <summary>
		/// Processes the specified image with the specified scale.
		/// </summary>
		/// <param name="imgUrl">The URL to the image.</param>
		/// <param name="scale">Either "true" or "false"</param>
		/// <returns>A list of names parsed.</returns>
		private static async Task<List<string>> ProcessImg(string imgUrl, string scale)
		{
			var parsePlayers = new Func<List<Line>, double, List<string>>((lines, avgEndpoint) =>
			{
				List<string> names = new();
				foreach (var line in lines)
				{
					if (line.LineText.Contains(":"))
						line.Words.RemoveRange(0, 
							line.Words.FindIndex(word => word.WordText.Contains(":")) + 1);

					for (int i = 0; i < line.Words.Count; i++)
					{
						Word word = line.Words[i];
						if (word.Left > avgEndpoint)
							break;
						names.Add(word.WordText.Replace(",", "")
							.Replace("0", "O")
							.Replace("1", "l")
							.Replace("|", "l").Trim());
					}
				}

#if DEBUG && !NO_PRINT
				Console.WriteLine($"[ParseImg:ParsePlayers]: {string.Join(", ", names)}");
#endif
				return names;
			});

			var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
			{
				new("url", imgUrl),
				new("language", "eng"),
				new("isOverlayRequired", "true"),
				new("scale", scale),
				new("OCREngine", "2")
			});

			using var reqRes = await Constants.OcrClient
				.PostAsync("https://api.ocr.space/parse/image", formContent);

#if DEBUG && !NO_PRINT
				Console.WriteLine(await reqRes.Content.ReadAsStringAsync());
#endif

			var json = JsonConvert.DeserializeObject<OcrSpaceResponse>(await reqRes.Content.ReadAsStringAsync());
			if (json is null || json.OcrExitCode != 1)
				return new List<string>();

			double left = -1;
			double top = -1;
			List<double> endpoints = new();
			List<Line> lines = new();

			// if line has words w/o comma - check if has next word ( if so, & nxt word.left < max, include), 

			// TODO missing "Pod" in 
			// https://cdn.discordapp.com/attachments/769659912941862972/770131676675440650/unknown.png
			// TODO unable to parse many names in
			// https://cdn.discordapp.com/attachments/737357260455542894/899060517086298132/image0.jpg
			foreach (var line in json.ParsedResults[0].TextOverlay.Lines)
			{
				var firstWord = line.Words[0];

				if (line.LineText.Contains("layers Online", StringComparison.OrdinalIgnoreCase))
				{
					if ((int) left != -1)
						lines.Clear();

					left = firstWord.Left;
					top = firstWord.Top;
					endpoints.Add(line.Words.Last().Left + line.Words.Last().Width);
					lines.Add(line);
#if DEBUG && !NO_PRINT
						Console.WriteLine($"[ParseImg:Bounds] Left: {left}, Top: {top}");
#endif
				}
				else if ((int) left != -1
				         && left - 8 < firstWord.Left
				         && firstWord.Left < left + 8 // condition fails
				         && firstWord.Top >= top
				         && Regex.IsMatch(line.LineText, "^[a-zA-Z, 01|]+$"))
				{
					if (line.Words.Last().WordText.Contains(","))
						endpoints.Add(line.Words.Last().Left + line.Words.Last().Width);
#if DEBUG && !NO_PRINT
						Console.WriteLine($"[ParseImg:Line] Testing Line: {line.LineText}");
#endif
					// Check if line is an output of /who
					// lines should contain only a-z (OCR sometimes replaces o's with zeros too).
					// lines should also contain at least one ',' (or only have 1 word)
					if (line.LineText.Contains(",") || line.Words.Count == 1)
						lines.Add(line);
				}
			}
			
			return parsePlayers(lines, endpoints.Average());
		}

		/// <summary>
		/// Parses a key screenshot.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns>The list of modifiers.</returns>
		[HttpPost("keymodifier")]
		public async Task<List<string>> ParseKeyScreenshot([FromBody] BaseUrlModel model)
		{
			Console.WriteLine("\n");
			_logger.LogInformation(
				$"[ParseKeyScreenshot] Received Key Screenshot\n"
				+ $"\t- URL: {model.Url}"
			);
			
			var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
			{
				new("url", model.Url),
				new("language", "eng"),
				new("isOverlayRequired", "true"),
				new("scale", "true"),
				new("OCREngine", "2")
			});

			using var reqRes = await Constants.OcrClient
				.PostAsync("https://api.ocr.space/parse/image", formContent);

#if DEBUG && !NO_PRINT
				Console.WriteLine(await reqRes.Content.ReadAsStringAsync());
#endif

			var json = JsonConvert.DeserializeObject<OcrSpaceResponse>(await reqRes.Content.ReadAsStringAsync());
			if (json is null || json.OcrExitCode != 1)
			{
				_logger.LogInformation(
					$"[ParseKeyScreenshot] Failed to Parse\n"
					+ $"\t- URL: {model.Url}\n"
					+ $"\t- Reason: OCR result either null or error exit code."
				);
				return new List<string>();
			}

			var returnList = new List<string>();
			foreach (var line in json.ParsedResults)
			{
				returnList.Add(line.ParsedText);
			}

			_logger.LogInformation(
				$"[ParseKeyScreenshot] Finished Parsing Key Screenshot\n"
				+ $"\t- URL: {model.Url}\n"
				+ $"\t- Results: {string.Join(" | ", returnList)}"
			);
			return returnList;
		}
	}

	internal static class LevenshteinDistance
	{
		public static int Compute(string s, string t)
		{
			if (string.IsNullOrEmpty(s))
				return string.IsNullOrEmpty(t) ? 0 : t.Length;

			if (string.IsNullOrEmpty(t)) return s.Length;

			var n = s.Length;
			var m = t.Length;
			var d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (var i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (var j = 1; j <= m; d[0, j] = j++)
			{
			}

			for (var i = 1; i <= n; i++)
			for (var j = 1; j <= m; j++)
			{
				var cost = t[j - 1] == s[i - 1] ? 0 : 1;
				var min1 = d[i - 1, j] + 1;
				var min2 = d[i, j - 1] + 1;
				var min3 = d[i - 1, j - 1] + cost;
				d[i, j] = Math.Min(Math.Min(min1, min2), min3);
			}

			return d[n, m];
		}
	}
}