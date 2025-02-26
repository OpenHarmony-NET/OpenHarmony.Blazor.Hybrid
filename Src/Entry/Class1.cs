using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OpenHarmony.NDK.Bindings.Native;

namespace Entry;

public class BlazorWebview : WebViewManager
{
    const string Scheme = "app";
    static readonly Uri BaseUri = new($"{Scheme}://localhost/");

    public BlazorWebview(IServiceProvider provider, IFileProvider fileProvider, string hostPageRelativePath) 
        : base(provider, Dispatcher.CreateDefault(), BaseUri, fileProvider, new(), hostPageRelativePath)
    {

    }
    
    protected override void NavigateCore(Uri absoluteUri)
    {
        Hilog.OH_LOG_INFO(LogType.LOG_APP, "blazor hybird", "NavigateCore: " + absoluteUri.ToString());
    }

    protected override void SendMessage(string message)
    {
        Hilog.OH_LOG_INFO(LogType.LOG_APP, "blazor hybird", "SendMessage: " + message);

    }
}