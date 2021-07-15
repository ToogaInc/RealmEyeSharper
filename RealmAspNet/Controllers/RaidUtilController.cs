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
	[Route("api/raidutil/parse")]
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
		/// </summary>
		/// <param name="model">The model. This should contain an URL.</param>
		/// <returns>The parse results.</returns>
		[HttpPost("img/")]
		public async Task<IActionResult> ParseImage([FromBody] ParseImgModel model)
		{
#if DEBUG
			Console.WriteLine("[ParseImg] Sending Image to OCR Endpoint.");
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

			var json = JsonConvert.DeserializeObject<OcrSpaceResponse>(await reqRes.Content.ReadAsStringAsync());
			if (json is null || json.OcrExitCode != 1)
				return Problem("OCR Processing Failed.");

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
					if (Regex.IsMatch(line.LineText, "^[a-zA-Z, 0]+"))
						players.AddRange(parsePlayers(line.LineText));
				}
			}

			if (players.Count == 0)
				return Problem("No players detected!");

			return await DoParseJob(players.ToArray());
		}


		/// <summary>
		/// Gets all the player's basic information (the player's RealmEye "homepage").
		/// </summary>
		/// <param name="names">The names.</param>
		/// <returns>The response.</returns>
		[HttpPost]
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
}