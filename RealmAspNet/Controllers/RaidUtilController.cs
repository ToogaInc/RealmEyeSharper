// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable CollectionNeverQueried.Global

#define NO_PRINT
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

// ReSharper disable SuggestBaseTypeForParameter

namespace RealmAspNet.Controllers
{
    [Route("api/raidutil")]
    [ApiController]
    public class RaidUtilController : ControllerBase
    {
        private static List<string> _defaultnames;
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
            _defaultnames = new List<string>
            {
                "darq", "deyst", "drac", "drol", "eango", "eashy", "eati", "eendi", "ehoni", "gharr", "iatho", "iawa",
                "idrae", "iri", "issz", "itani", "laen", "lauk", "lorz", "oalei", "odaru", "oeti", "orothi", "oshyu",
                "queq", "radph", "rayr", "ril", "rilr", "risrr", "saylt", "scheev", "sek", "serl", "seus", "tal",
                "tiar", "uoro", "urake", "utanu", "vorck", "vorv", "yangu", "yimi", "zhiar"
            };
        }


        /// <summary>
        ///     Parses a /who screenshot. Uses OCR.space's OCR API to get text from a /who screenshot.
        /// </summary>
        /// <param name="model">The model. This should contain an URL.</param>
        /// <returns>The parse results.</returns>
        [HttpPost("parseWhoOnly")]
        public async Task<IActionResult> ParseWhoScreenshotAsync([FromBody] WhoParseOnlyModel model)
        {
            var jobId = _apiReqJobId++;
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

            // If we're only interested in basic /who parsing, then run this and return.
            _logger.LogInformation(
                $"[ParseImgWhoOnly] [ID {jobId}] /who Parsing Successful. Time: {stopwatch.Elapsed.TotalSeconds} Seconds."
            );

            return Ok(names);
        }

        /// <summary>
        ///     Parses a /who screenshot and gets all the player's basic information (the player's RealmEye "homepage").
        ///     Uses OCR.space's OCR API to get text from a /who screenshot.
        /// </summary>
        /// <param name="model">The model. This should contain an URL.</param>
        /// <returns>The parse results.</returns>
        [HttpPost("parseWhoRealmEye")]
        public async Task<IActionResult> ParseWhoScreenshotAndGetDataAsync([FromBody] ParseImgModel model)
        {
            var jobId = _apiReqJobId++;
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

                var reReq = await SendConcurrentRealmEyeRequestsAsync(names.ToArray());
                reReq.ElapsedSec = stopwatch.Elapsed.TotalSeconds;
                _logger.LogInformation(
                    $"[ParseImgGetData:Simple] [ID {jobId}] /who Parsing + RE Data Request Successful.\n"
                    + $"\t- Time: {reReq.ElapsedSec} Seconds."
                );
                return Ok(reReq);
            }

            // Otherwise, we're going to do more advanced /who parsing and get RE data.
            var scaledTask = ProcessImg(model.Url, "true");
            var allNames = await Task.WhenAll(unscaledTask, scaledTask);

            _logger.LogInformation(
                $"[ParseImgGetData] [ID {jobId}] OCR Parse Completed in {stopwatch.Elapsed.TotalSeconds} Seconds."
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
            unscaledRes.Input = newInput;
            unscaledRes.Completed.AddRange(fixedNamesRes.Completed);
            unscaledRes.CompletedCount = unscaledRes.Completed.Count;
            unscaledRes.FailedCount = unscaledRes.Failed.Count;
            unscaledRes.Output.AddRange(fixedNamesRes.Output);

            if (unscaledRes.CompletedCount == 0)
                return Problem("No players detected!");

            _logger.LogInformation(
                $"[ParseImgGetData] [ID {jobId}] /who Parsing Successful. Time: {stopwatch.Elapsed.TotalSeconds} Seconds."
            );

            stopwatch.Stop();
            unscaledRes.ElapsedSec = stopwatch.Elapsed.TotalSeconds;
            return Ok(unscaledRes);
        }


        /// <summary>
        ///     Gets all the player's basic information (the player's RealmEye "homepage").
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>The response.</returns>
        [HttpPost("namesRealmEye")]
        public async Task<IActionResult> RequestMultipleRealmEyeProfilesAsync([FromBody] string[] names)
        {
            return Ok(await SendConcurrentRealmEyeRequestsAsync(names));
        }

        /// <summary>
        ///     Sends multiple RealmEye requests.
        /// </summary>
        /// <param name="names">The names to get data for.</param>
        /// <returns>The job result.</returns>
        private async Task<ParseJob> SendConcurrentRealmEyeRequestsAsync(string[] names)
        {
            var jobId = _concurrJobId++;
            _logger.LogInformation(
                $"[ConcurRealmEyeReq] [ID {jobId}] Started Job.\n"
                + $"\t- Name Count: {names.Length}"
            );

            var stopwatch = Stopwatch.StartNew();

            var defaults = names.Intersect(_defaultnames, StringComparer.OrdinalIgnoreCase).ToList();
            names = names.Except(defaults).ToArray();

            var job = new ParseJob
            {
                ElapsedSec = 0,
                CompletedCount = 0,
                FailedCount = 0,
                Output = new List<PlayerData>(),
                Completed = new List<string>(),
                Failed = new List<string>(),
                DefaultNames = defaults,
                Input = names.ToList()
            };

            var bag = new ConcurrentBag<PlayerData>();

            var block = new ActionBlock<string>(async name =>
                    bag.Add(await PlayerScraper.ScrapePlayerProfileAsync(name)),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 40
                });

            foreach (var name in names) await block.SendAsync(name);

            block.Complete();
            await block.Completion;

            var profiles = bag.ToList();

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
            job.ElapsedSec = stopwatch.Elapsed.TotalSeconds;
            job.CompletedCount = job.Completed.Count;
            job.FailedCount = job.Failed.Count;

            _logger.LogInformation(
                $"[ConcurRealmEyeReq] [ID {jobId}] Finished Job.\n"
                + $"\t- Time: {job.ElapsedSec} Seconds.\n"
                + $"\t- Completed: {job.CompletedCount}\n"
                + $"\t- Failed: {job.FailedCount} ({string.Join(", ", job.Failed)})"
            );
            return job;
        }

        /// <summary>
        ///     Processes the specified image with the specified scale.
        /// </summary>
        /// <param name="imgUrl">The URL to the image.</param>
        /// <param name="scale">Either "True" or "False"</param>
        /// <returns>A list of names parsed.</returns>
        private static async Task<List<string>> ProcessImg(string imgUrl, string scale)
        {
            var parsePlayers = new Func<string, List<string>>(str =>
            {
                if (str.Contains(":"))
                    str = str[(str.IndexOf(":", StringComparison.Ordinal) + 1)..];

                var split = str.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                if (split.Count > 0)
                    for (var i = 0; i < split.Count; i++)
                    {
                        if (i < split.Count - 1)
                            split[i] = split[i].Replace(" ", "");
                        else
                            split[i] = split[i].TrimStart();
                        split[i] = split[i].Replace("0", "o");
                    }

#if DEBUG && !NO_PRINT
				Console.WriteLine($"[ParseImg:ParsePlayers]: {string.Join(", ", split)}");
#endif

                return split;
            });

            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("url", imgUrl),
                new("language", "eng"),
                new("isOverlayRequired", "true"),
                new("scale", scale),
                new("OCREngine", "2")
            });
            using var reqRes =
                await Constants.OCRClient.PostAsync("https://api.ocr.space/parse/image", formContent);

#if DEBUG && !NO_PRINT
				Console.WriteLine(await reqRes.Content.ReadAsStringAsync());
#endif

            var json = JsonConvert.DeserializeObject<OcrSpaceResponse>(await reqRes.Content.ReadAsStringAsync());
            if (json is null || json.OcrExitCode != 1)
                return new List<string>();

            double left = -1;
            double top = -1;
            var players = new List<string>();

            foreach (var line in json.ParsedResults[0].TextOverlay.Lines)
            {
                var firstWord = line.Words[0];

                if (line.LineText.Contains("layers Online", StringComparison.OrdinalIgnoreCase))
                {
                    if ((int) left != -1)
                        players.Clear();

                    left = firstWord.Left;
                    top = firstWord.Top;
                    players.AddRange(parsePlayers(line.LineText));
#if DEBUG && !NO_PRINT
						Console.WriteLine($"[ParseImg:Bounds] Left: {left}, Top: {top}");
#endif
                }
                else if ((int) left != -1
                         && left - 8 < firstWord.Left
                         && firstWord.Left < left + 8
                         && firstWord.Top >= top)
                {
#if DEBUG && !NO_PRINT
						Console.WriteLine($"[ParseImg:Line] Testing Line: {line.LineText}");
#endif
                    // Check if line is an output of /who
                    // lines should contain only a-z (OCR sometimes replaces o's with zeros too).
                    // lines should also contain at least one ',' (or only have 1 word)
                    if ((line.LineText.Contains(",") || line.Words.Count == 1)
                        && Regex.IsMatch(line.LineText, "^[a-zA-Z, 0]+$"))
                        players.AddRange(parsePlayers(line.LineText));
                }
            }

            return players;
        }
    }


    internal class ParseJob
    {
        public double ElapsedSec { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Input { get; set; }
        public List<string> Completed { get; set; }
        public List<string> Failed { get; set; }
        public List<string> DefaultNames { get; set; }
        public List<PlayerData> Output { get; set; }
    }

    internal static class LevenshteinDistance
    {
        public static int Compute(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

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