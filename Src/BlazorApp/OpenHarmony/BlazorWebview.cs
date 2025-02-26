using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
// using OpenHarmony.NDK.Bindings.Native;

namespace BlazorApp.OpenHarmony;

public class BlazorWebview : WebViewManager
{
    const string Scheme = "app";
    static readonly Uri BaseUri = new($"{Scheme}://localhost/");
    const string hostPageRelativePath = "index.html";
    public BlazorWebview(IServiceProvider provider, IFileProvider fileProvider)
        : base(provider, Dispatcher.CreateDefault(), BaseUri, fileProvider, new(), hostPageRelativePath)
    {

    }

    protected override void NavigateCore(Uri absoluteUri)
    {
        //  Hilog.OH_LOG_INFO(LogType.LOG_APP, "blazor hybird", "NavigateCore: " + absoluteUri.ToString());
    }

    protected override void SendMessage(string message)
    {
        //  Hilog.OH_LOG_INFO(LogType.LOG_APP, "blazor hybird", "SendMessage: " + message);

    }
}