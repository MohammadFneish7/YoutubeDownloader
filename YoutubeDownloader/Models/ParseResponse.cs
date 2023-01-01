namespace YoutubeDownloader.Models
{
    public class ParseResponse
    {
        public long CountRequests { get; set; }

        public List<FormatInfo> FormatInfos { get; set; }
    }
}
