using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();

// You can specify either the video URL or its ID
var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";
var video = await youtube.Videos.GetAsync(videoUrl);

var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
var author = video.Author.ChannelTitle; // "Blender"
var duration = video.Duration; // 00:07:20

var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

var lowestAudio = streamManifest.GetAudioOnlyStreams().OrderBy(x => x.Bitrate.BitsPerSecond).First();
var rb = new ConversionRequestBuilder($"C:\\Users\\96170\\Desktop\\video.mp3").SetContainer("")
    .SetFFmpegPath("C:\\Users\\96170\\Desktop\\ffmpeg-2023-03-05-git-912ac82a3c-full_build\\bin\\ffmpeg.exe")
    .SetFormat("mp3")
    .SetPreset(ConversionPreset.UltraFast).Build();
await youtube.Videos.Streams.DownloadAsync(lowestAudio,$"C:\\Users\\96170\\Desktop\\video2.mp3");
//await youtube.Videos.DownloadAsync(new[] { lowestAudio }.ToList(), rb);

Console.WriteLine("Hello, World!");
