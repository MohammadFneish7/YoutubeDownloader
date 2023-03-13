using System.Collections.Generic;
using Yarp.ReverseProxy.Forwarder;

namespace YoutubeDownloader.Helpers
{
    public class CustomHttpTransformer : HttpTransformer
    {
        public string Meme { get; set; }
        public CustomHttpTransformer(string meme)
        {
            Meme = meme;
        }

        public override ValueTask<bool> TransformResponseAsync(HttpContext httpContext, HttpResponseMessage? proxyResponse, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(Meme))
                httpContext.Response.Headers.ContentType = Meme;
            return base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);
        }
    }
}
