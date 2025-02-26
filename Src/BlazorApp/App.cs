using BlazorApp.OpenHarmony;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OpenHarmony.NDK.Bindings.Native;

namespace BlazorApp;

public class App
{
    public static BlazorWebview Create()
    {

        var serviceProvider = new ServiceCollection();
        serviceProvider.AddBlazorWebView();
        var provider = serviceProvider.BuildServiceProvider();

        var webview = new BlazorWebview(provider, new PhysicalFileProvider("/data/storage/el1/bundle/entry/resources/resfile/wwwroot"));

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", "App Create");

        return webview;
    }

   
}
