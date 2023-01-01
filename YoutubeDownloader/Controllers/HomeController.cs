using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using YoutubeDownloader.Models;

namespace YoutubeDownloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Counter Counter;
        public HomeController(ILogger<HomeController> logger, Counter counter)
        {
            _logger = logger;
            Counter = counter;
        }
        [Route("")]
        [Route("watch/{v?}")]
        [Route("Home")]
        [Route("Home/Index")]
        public IActionResult Index(string? v)
        {
            if(!string.IsNullOrEmpty(v))
                ViewBag.url = "https://www.youtube.com/watch?v=" + v;
            else
                ViewBag.url = string.Empty;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public string Parse([FromQuery] string videourl)
        {
            try
            {
                ParseResponse parseResponse = new ParseResponse();
                parseResponse.CountRequests = Counter.Increment();

                var output = Execute_ytdl($"{videourl} --list-formats");

                var infos = new List<FormatInfo>();
                var start = false;
                foreach (var line in output.Split('\n'))
                {
                    if (!start && line.Contains("format code  extension  resolution note"))
                    {
                        start = true;
                        continue;
                    }

                    if (!start)
                        continue;

                    if (line.Contains("video only"))
                        continue;

                    var pline = line.Replace("audio only", "Audio").Replace("(best)", "").Trim();
                    var fi = new FormatInfo();

                    var parts = Regex.Split(pline, "[ \\\\t]+");
                    if(parts.Length >= 5)
                    {
                        fi.ID = parts[0];
                        fi.Extension = parts[1];
                        fi.Resolution = parts[2];
                        if (fi.Resolution.Trim().Equals("Audio"))
                            fi.Icon = "bi-mic-fill";
                        else
                            fi.Icon = "bi-camera-reels-fill";
                        fi.Bitrate = parts[4];
                        fi.Size = parts[parts.Length - 1];
                        fi.MEME = (fi.Resolution.Equals("audio", StringComparison.InvariantCultureIgnoreCase) ? "audio" : "video") + "/" + fi.Extension;
                        infos.Add(fi);
                    }
                }

                parseResponse.FormatInfos = infos;

                return JsonConvert.SerializeObject(parseResponse);

            }
            catch (Exception ex)
            {
                return $"{ex}";
            }
        }

        [HttpPost]
        public string Download([FromQuery] string videourl, string formatId)
        {
            try
            {
                
                var ouptut = Execute_ytdl($"-f {formatId} {videourl} -g");

                return ouptut;

            }
            catch (Exception ex)
            {
                return $"{ex}";
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string Execute_ytdl(string args)
        {
            string binName = "ytdl/ytdl.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                binName = "ytdl/youtube-dl";
            }
            Process cmd = new Process();
            cmd.StartInfo.FileName = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, binName);
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.Arguments = args;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.WaitForExit();
            string? err = cmd.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(err))
                throw new Exception(err);

            return cmd.StandardOutput.ReadToEnd();
        }
    }
}