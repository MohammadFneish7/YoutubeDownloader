using System.Diagnostics;
using System.Net;
using System.Text;
using Yarp.ReverseProxy.Forwarder;
using YoutubeDownloader.Helpers;
using YoutubeDownloader.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddSingleton<Counter>();
builder.Services.AddHttpForwarder();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".YoutubeDownloader.Session";
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseSession();

var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(30),
});

var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

app.UseEndpoints(endpoints =>
{
    endpoints.Map("/download/{**catch-all}", async httpContext =>
    {
        string meme = "";
        var query = Encoding.UTF8.GetString(Convert.FromBase64String(httpContext.Request.QueryString.Value.Substring(3)));
        if (query.Contains("googlevideo.com"))
        {
            var parts = query.Split("&downfilename=");
            var filename = parts[1].Split(";")[0];
            string contentDisposition = $"attachment; filename={filename}";
            if (Encoding.UTF8.GetByteCount(filename) != filename.Length) {
                var filenamepart = Convert.ToBase64String(Encoding.UTF8.GetBytes(filename.Substring(0, filename.LastIndexOf("."))));
                filenamepart = filenamepart.Substring(0, Math.Min(filenamepart.Length, 25));
                contentDisposition = $"attachment; filename={filenamepart + filename.Substring(filename.LastIndexOf("."), filename.Length - filename.LastIndexOf("."))}";
            }
            meme = parts[1].Split(";")[1];
            var hst = parts[0].Split("//")[1];
            httpContext.Request.Host = new HostString(hst.Split("/")[0]);
            httpContext.Request.QueryString = new QueryString(hst.Split("/")[1].Substring(13));
            httpContext.Request.Path = "/videoplayback";
            httpContext.Response.Headers.ContentType = meme;
            httpContext.Response.Headers.Add("Content-Disposition", contentDisposition);
        }

        var error = await app.Services.GetService<IHttpForwarder>().SendAsync(httpContext, "https://" + httpContext.Request.Host.Value + "/",
            httpClient, requestConfig, new CustomHttpTransformer(meme));

        if (error != ForwarderError.None)
        {
            var errorFeature = httpContext.GetForwarderErrorFeature();
            var exception = errorFeature.Exception;
        }
    });
});

//app.UseEndpoints(endpoint => endpoint.MapDefaultControllerRoute());

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
