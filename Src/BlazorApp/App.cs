using BlazorApp.OpenHarmony;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OpenHarmony.NDK.Bindings.Native;
using Microsoft.Extensions.Logging;

namespace BlazorApp;

public class App
{
    public static BlazorWebview Create(napi_env Env, napi_value sendMessage, napi_value navigateCore)
    {

        var serviceProvider = new ServiceCollection();
        serviceProvider.AddBlazorWebView();
        serviceProvider.AddLogging(lb =>
        {
        });
        var provider = serviceProvider.BuildServiceProvider();
        var webview = new BlazorWebview(provider, new BlaozrDispatcher(), new PhysicalFileProvider("/data/storage/el1/bundle/entry/resources/resfile/wwwroot"), Env, sendMessage, navigateCore);

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", "App Create");

        return webview;
    }

   
}
