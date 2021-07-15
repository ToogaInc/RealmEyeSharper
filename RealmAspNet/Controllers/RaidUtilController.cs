// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable CollectionNeverQueried.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RealmAspNet.Controllers
{
	[Route("api/raidutil")]
	[ApiController]
	public class RaidUtilController : ControllerBase
	{
		private readonly ILogger<RaidUtilController> _logger;
		private int _jobCount;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RaidUtilController(ILogger<RaidUtilController> logger)
		{
			_jobCount = 0;
			_logger = logger;
		}

		/// <summary>
		/// Parses a /who screenshot and gets all the player's basic information (the player's RealmEye "homepage").
		/// Uses OCR.space's OCR API to get text from a /who screenshot.
		/// </summary>
		/// <param name="model">The model. This should contain an URL.</param>
		/// <returns>The parse results.</returns>
		[HttpPost("parseWho")]
		public async Task<IActionResult> ParseWhoScreenshotAndGetDataAsync([FromBody] ParseImgModel model)
		{
			var stopwatch = Stopwatch.StartNew();
#if DEBUG
			Console.WriteLine("[ParseImg] Sending Image to OCR Endpoint.");
#endif
			
			var parsePlayers = new Func<string, List<string>>(str =>
			{
				if (str.Contains(":"))
					str = str[(str.IndexOf(":", StringComparison.Ordinal) + 1)..];

				var split = str.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
				for (var i = 0; i < split.Count; i++)
					split[i] = split[i].Replace(" ", "").Replace("0", "o");

#if DEBUG
				Console.WriteLine($"[ParseImg:ParsePlayers]: {string.Join(", ", split)}");
#endif

				return split;
			});

			async Task<List<string>> ProcessImg(string scale)
			{
				var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
				{
					new("url", model.Url),
					new("isOverlayRequired", "true"),
					new("scale", scale),
					new("OCREngine", "2")
				});
				using var reqRes = await Constants.OCRClient.PostAsync("https://api.ocr.space/parse/image", formContent);

#if DEBUG
				Console.WriteLine(await reqRes.Content.ReadAsStringAsync());
#endif

				var json = JsonConvert.DeserializeObject<OcrSpaceResponse>(await reqRes.Content.ReadAsStringAsync());
				if (json is null || json.OcrExitCode != 1)
					return null;

				double left = -1;
				double top = -1;
				var players = new List<string>();

				foreach (var line in json.ParsedResults[0].TextOverlay.Lines)
				{
					var firstWord = line.Words[0];

					if (line.LineText.Contains("Players Online", StringComparison.OrdinalIgnoreCase))
					{
						if ((int) left != -1)
							players.Clear();

						left = firstWord.Left;
						top = firstWord.Top;
						players.AddRange(parsePlayers(line.LineText));
#if DEBUG
						Console.WriteLine($"[ParseImg:Bounds] Left: {left}, Top: {top}");
#endif
					}
					else if ((int) left != -1
					         && left - 8 < firstWord.Left
					         && firstWord.Left < left + 8
					         && firstWord.Top >= top)
					{
#if DEBUG
						Console.WriteLine($"[ParseImg:Line] Testing Line: {line.LineText}");
#endif				
						// Check if line is an output of /who
						// lines should contain only a-z (OCR sometimes replaces o's with zeros too).
						// lines should also contain at least one ',' (or only have 1 word)
						if ((line.LineText.Contains(",") &&
						     Regex.IsMatch(line.LineText, "^[a-zA-Z, 0]+")) ||
						    line.Words.Count == 1)  
							players.AddRange(parsePlayers(line.LineText));
					}
				}

				return players;
			}

			var unscaledTask = ProcessImg("false");
			var scaledTask = ProcessImg("true");
			var allNames = await Task.WhenAll(unscaledTask, scaledTask);

			var unscaledPlayers = allNames[0];
			var scaledPlayers = allNames[1];
			
#if DEBUG
			Console.WriteLine("[ParseImg:Players] Player List (Common): " + 
			                  String.Join(", ", unscaledPlayers));
			Console.WriteLine("[ParseImg:Players] Player List (Diff.): " + 
			                  String.Join(", ", scaledPlayers));
#endif

			var unscaledRes = await SendConcurrentRealmEyeRequestsAsync(unscaledPlayers.ToArray());


			Dictionary<string, string> fixedNames = new();
			List<string> notFound = new();
			
			foreach (var name in unscaledRes.Failed)
			{
				int minDist = 5;
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
				else
					notFound.Add(name);
			}

			var fixedNamesRes = await SendConcurrentRealmEyeRequestsAsync(fixedNames.Values.ToArray());

			var newInput = unscaledRes.Input.ToList();
			foreach (var name in unscaledRes.Input)
			{
				if (fixedNames.ContainsKey(name) && fixedNamesRes.Completed.Contains(fixedNames[name]))
				{
					if (unscaledRes.Failed.Contains(name))
						unscaledRes.Failed.Remove(name);
					newInput.Remove(name);
					newInput.Add(fixedNames[name]);
				}
			}
			
			unscaledRes.Input = newInput;
			unscaledRes.Completed.AddRange(fixedNamesRes.Completed);
			unscaledRes.CompletedCount = unscaledRes.Completed.Count;
			unscaledRes.FailedCount = unscaledRes.Failed.Count;
			unscaledRes.Output.AddRange(fixedNamesRes.Output);

			if (unscaledRes.CompletedCount == 0)
				return Problem("No players detected!");
			
			_logger.LogInformation(
				$"[ParseImg] /who Parsing Successful. Time: {stopwatch.Elapsed.TotalSeconds} Seconds."
			);
			
			stopwatch.Stop();
			unscaledRes.Elapsed = stopwatch.Elapsed.TotalSeconds;
			
			if (!model.GetRealmEyeData ?? true)
				return Ok(unscaledPlayers);

			return Ok(unscaledRes);
		}


		/// <summary>
		/// Gets all the player's basic information (the player's RealmEye "homepage").
		/// </summary>
		/// <param name="names">The names.</param>
		/// <returns>The response.</returns>
		[HttpPost("parseNamesForREProfiles")]
		public async Task<IActionResult> RequestMultipleRealmEyeProfilesAsync([FromBody] string[] names)
			=> Ok(await SendConcurrentRealmEyeRequestsAsync(names));

		/// <summary>
		/// Sends multiple RealmEye requests.
		/// </summary>
		/// <param name="names">The names to get data for.</param>
		/// <returns>The job result.</returns>
		private async Task<ParseJob> SendConcurrentRealmEyeRequestsAsync(string[] names)
		{
			var jobId = _jobCount++;
			_logger.LogInformation(
				$"[SendConcurrentRealmEyeRequests] Started Job {jobId}. Name Count: {names.Length}"
			);
			
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

			_logger.LogInformation(
				$"[SendConcurrentRealmEyeRequests] Finished RE Job {jobId}. Time: {job.Elapsed} Seconds.\n"
				+ $"\t- Completed: {job.CompletedCount}\n"
				+ $"\t- Failed: {job.FailedCount} ({string.Join(", ", job.Failed)})"
			);
			return job;
		}
	}


	internal class ParseJob
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
	
	internal class LevenshteinDistance
	{
		public static int Compute(string s, string t)
		{
			if (string.IsNullOrEmpty(s))
			{
				if (string.IsNullOrEmpty(t))
					return 0;
				return t.Length;
			}

			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++);
			for (int j = 1; j <= m; d[0, j] = j++);

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
	}
	
}