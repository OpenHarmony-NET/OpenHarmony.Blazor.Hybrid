using BlazorApp.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using OpenHarmony.NDK.Bindings.Native;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;
// using OpenHarmony.NDK.Bindings.Native;

namespace BlazorApp.OpenHarmony;

public class BlazorWebview : WebViewManager
{
    public napi_env Env;

    public napi_value sendMessage;

    public napi_value navigateCore;

    public BlaozrDispatcher dispatcher;

    const string Scheme = "https";
    static readonly Uri BaseUri = new($"{Scheme}://localhost/");
    const string hostPageRelativePath = "index.html";
    public BlazorWebview(IServiceProvider provider, BlaozrDispatcher dispatcher, IFileProvider fileProvider, napi_env env, napi_value sendMessage, napi_value navigateCore)
        : base(provider, dispatcher, BaseUri, fileProvider, new(), hostPageRelativePath)
    {
        this.dispatcher = dispatcher;
        this.Env = env;
        this.sendMessage= sendMessage;
        this.navigateCore = navigateCore;

        AddRootComponentAsync(typeof(Routes), "#app", ParameterView.Empty);
        test();
    }


    async void test()
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(1000);
            Hilog.OH_LOG_INFO(LogType.LOG_APP, "BlazorHybrid", "test dispatcher: " + i);
        }
    }
    public unsafe napi_value InterceptRequest(napi_env env, string url, napi_value createResponse)
    {
        if (this.TryGetResponseContent(url, false, out var statusCode, out var statusMessage, out var stream, out var headers))
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var contentmessage = ms.ToArray();
            var content = Encoding.UTF8.GetString(contentmessage);
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", $"url: {url}, \nstatusCode: {statusCode}, statusMessage: {statusMessage}");
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", $"content: {content}");
            napi_value* args = stackalloc napi_value[4];

            ace_napi.napi_create_int32(env, statusCode, args);



            var messageBytes = Encoding.UTF8.GetBytes(statusMessage);
            fixed (void* p = messageBytes)
            {
                ace_napi.napi_create_string_utf8(env, (sbyte*)p, (ulong)messageBytes.Length, args + 1);
            }

            var mimeTypeBytes = Encoding.UTF8.GetBytes(headers["Content-Type"]);
            fixed (void* p = mimeTypeBytes)
            {
                if (ace_napi.napi_create_string_utf8(env, (sbyte*)p, (ulong)mimeTypeBytes.Length, args + 2) != napi_status.napi_ok)
                {
                    Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "InterceptRequest create string failed");
                    return default;
                }
            }

            byte* data = null;
            if (ace_napi.napi_create_arraybuffer(env, (ulong)contentmessage.Length, (void**)&data, args + 3) != napi_status.napi_ok)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "InterceptRequest create arraybuffer failed");
                return default;
            }
            else
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", $"contentmessage.Length: {(ulong)contentmessage.Length}");
                for (int i = 0; i < contentmessage.Length; i++)
                {
                    data[i] = contentmessage[i];
                }
            }
            napi_value response = default;
            if (ace_napi.napi_call_function(env, default, createResponse, 4, args, &response) != napi_status.napi_ok)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "InterceptRequest call function failed");
                return default;
            }

            return response;
        }
        else
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", $"url: {url}, \nTryGetResponseContent: failed");
        }
        return default;
        //  Hilog.OH_LOG_INFO(LogType.LOG_APP, "blazor hybird", "InterceptRequest: " + url);
    }
    protected unsafe override void NavigateCore(Uri absoluteUri)
    {
        Hilog.OH_LOG_INFO(LogType.LOG_APP, "BlazorHybrid", "NavigateCore: " + absoluteUri.ToString());
        var data = Encoding.UTF8.GetBytes(absoluteUri.ToString());
        napi_value url = default;
        napi_status code = default;
        fixed (void* p = data)
        {
            code = ace_napi.napi_create_string_utf8(Env, (sbyte*)p, (ulong)data.Length, &url);

            if (code != napi_status.napi_ok)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "NavigateCore create string failed, code: " + code);
            }
        }
        code = ace_napi.napi_call_function(Env, default, navigateCore, 1, &url, default);
        if (code != napi_status.napi_ok)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "NavigateCore call function failed, code: " + code);
        }
    }

    protected unsafe override void SendMessage(string message)
    {
        Hilog.OH_LOG_INFO(LogType.LOG_APP, "BlazorHybrid", "SendMessage: " + message);
        var data = Encoding.UTF8.GetBytes(message.ToString());
        napi_value msg = default;
        fixed (void* p = data)
        {
            ace_napi.napi_create_string_utf8(Env, (sbyte*)p, (ulong)data.Length, &msg);
        }
        var code = ace_napi.napi_call_function(Env, default, sendMessage, 1, &msg, default);
        if (code != napi_status.napi_ok)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "SendMessage call function failed code: " + code);
        }


    }

    public void OnFrame()
    {
        dispatcher.Tick();
    }
    public void MessageReceived(string message)
    {
        Hilog.OH_LOG_INFO(LogType.LOG_APP, "BlazorHybrid", "MessageReceived: " + message);
        MessageReceived(BaseUri, message);
    }
}