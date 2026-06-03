using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Photino.NET;
using TransmissonNET.App;
using TransmissonNET.App.Api;
using TransmissonNET.App.Desktop;
using TransmissonNET.Application;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Infrastructure;

var launchTorrentPath = CommandLineTorrentLaunch.FindTorrentPath(args);
if (SingleInstanceHost.TryForwardToRunningInstance(launchTorrentPath))
    return;

var app = WebAppFactory.Build(args);

if (!string.IsNullOrEmpty(launchTorrentPath))
    app.Services.GetRequiredService<IPendingTorrentLaunchStore>().SetPendingPath(launchTorrentPath);

if (app.Environment.IsEnvironment("Testing"))
{
    await app.RunAsync();
    return;
}

await DesktopHost.RunAsync(app);

internal static class WebAppFactory
{
    public static WebApplication Build(string[] args)
    {
        var builder = CreateBuilder(args);
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddTransmissonNetApplication();
        builder.Services.AddTransmissonNetInfrastructure();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        app.MapTransmissonNetApi();

        return app;
    }

    private static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var testing = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Testing",
            StringComparison.OrdinalIgnoreCase);
        if (testing)
            return WebApplication.CreateBuilder(args);

        var appDir = AppContext.BaseDirectory;
        var wwwroot = Path.Combine(appDir, "wwwroot");
        if (!Directory.Exists(wwwroot))
            return WebApplication.CreateBuilder(args);

        return WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = appDir,
            WebRootPath = wwwroot,
        });
    }
}

internal static class DesktopHost
{
    public static async Task RunAsync(WebApplication app)
    {
        await app.StartAsync();

        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var baseUrl = addresses?.FirstOrDefault()
            ?? throw new InvalidOperationException("Kestrel did not bind to any address.");

        Console.WriteLine($"TransmissionNET listening on {baseUrl}");
        Console.WriteLine($"UI in browser: {baseUrl}");

        LinuxDisplayBootstrap.Configure();

        var pendingStore = app.Services.GetRequiredService<IPendingTorrentLaunchStore>();
        await using var singleInstance = new SingleInstanceHost(
            pendingStore,
            DesktopWindowActivator.TryActivate);
        singleInstance.Start();

        var window = new PhotinoWindow()
            .SetTitle("TransmissionNET")
            .SetUseOsDefaultSize(false)
            .SetSize(1280, 800)
            .Center()
            .Load(baseUrl);

        window.WaitForClose();

        await app.StopAsync();
    }
}
