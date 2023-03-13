using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using YoutubeDownloader.Helpers;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Counter Counter;
        private readonly string pass = "ZSUyQ2dpciUyQ2NsZW4lMkNkdXIlMkNsbXQmc2lnPU";
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

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token")))
            {
                var token = Cryptography.Encrypt(DateTime.Now.AddHours(1).ToString(), pass);
                HttpContext.Session.SetString("access_token", token);
            }
            else
            {
                var token = Cryptography.Decrypt(HttpContext.Session.GetString("access_token"), pass);
                var exp = DateTime.Parse(token);
                if(exp < DateTime.Now)
                {
                    token = Cryptography.Encrypt(DateTime.Now.AddHours(1).ToString(), pass);
                    HttpContext.Session.SetString("access_token", token);
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Parse([FromQuery] string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token")))
                {
                    return Forbid();
                }
                else
                {
                    var token = Cryptography.Decrypt(HttpContext.Session.GetString("access_token"), pass);
                    var exp = DateTime.Parse(token);
                    if (exp < DateTime.Now)
                    {
                        return Forbid();
                    }
                }

                ParseResponse parseResponse = new ParseResponse();
                parseResponse.CountRequests = Counter.Increment();

                var youtube = new YoutubeClient();

                var video = await youtube.Videos.GetAsync(videoUrl);

                var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    title = title.Replace(c, '_');
                }
                var author = video.Author.ChannelTitle; // "Blender"
                var duration = video.Duration; // 00:07:20
                var meme = "";

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var infos = new List<FormatInfo>();
                int id = 0;
                bool hasMp3 = false;
                foreach(var inf in streamManifest.Streams)
                {
                    id++;
                    string cont = inf.Container.ToString();
                    var fi = new FormatInfo();
                    if(inf.GetType() == typeof(MuxedStreamInfo))
                    {
                        meme = "video/";
                        fi.MEME = "Video";
                        fi.Icon = "bi-camera-reels-fill";
                        fi.Resolution = ((MuxedStreamInfo)inf).VideoResolution.ToString();
                    }
                    else if (inf.GetType() == typeof(VideoOnlyStreamInfo))
                    {
                        meme = "video/";
                        fi.MEME = "Video Only";
                        fi.Icon = "bi-camera-reels-fill";
                        fi.Resolution = ((VideoOnlyStreamInfo)inf).VideoResolution.ToString();
                    }
                    else if (inf.GetType() == typeof(AudioOnlyStreamInfo))
                    {
                        if (!hasMp3)
                        {
                            cont = "mp3";
                            hasMp3 = true;
                        }
                        meme = "audio/";
                        fi.MEME = "Audio";
                        fi.Icon = "bi-mic-fill";
                        fi.Resolution = "";
                    }
                    meme += cont;
                    fi.Extension = cont;
                    fi.Bitrate = inf.Bitrate.ToString();
                    fi.Url = "/download?v=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(inf.Url + "&downfilename=" + title + "." + fi.Extension + ";" + meme));
                    fi.Size = inf.Size.ToString();
                    infos.Add(fi);
                }

                parseResponse.FormatInfos = infos;

                return Ok(JsonConvert.SerializeObject(parseResponse));

            }
            catch (Exception ex)
            {
                return Ok($"{ex}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}