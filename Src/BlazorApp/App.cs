using BlazorApp.OpenHarmony;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorApp;

public class App : IHostedService
{
    BlazorWebview Webview;
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddBlazorWebView().AddHostedService<App>();
        builder.Build().Run();
    }

    public App(IHostApplicationLifetime lifetime, IServiceProvider serviceProvider)
    {
        Webview = new BlazorWebview(serviceProvider, null);
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
