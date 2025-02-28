using BlazorApp.OpenHarmony;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OpenHarmony.NDK.Bindings.Native;
using Microsoft.Extensions.Logging;

namespace BlazorApp;

public class App
{
    public static BlazorWebview Create(napi_env Env, napi_ref sendMessage, napi_ref navigateCore)
    {

        var serviceProvider = new ServiceCollection();
        serviceProvider.AddBlazorWebView();
        serviceProvider.AddLogging(lb =>
        {
        });
        var provider = serviceProvider.BuildServiceProvider();
        var contenRoot = "/data/storage/el1/bundle/entry/resources/resfile/wwwroot";
        if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64)
        {
            contenRoot += "/x86_64";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
        {
            contenRoot += "/arm64-v8a";
        }
        else
        {
            throw new Exception("Unsupported Architecture");
        }
        var webview = new BlazorWebview(provider, new BlaozrDispatcher(), new PhysicalFileProvider(contenRoot), Env, sendMessage, navigateCore);

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", "App Create");

        return webview;
    }

   
}
