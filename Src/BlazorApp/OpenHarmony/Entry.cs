﻿using OpenHarmony.NDK.Bindings.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.OpenHarmony;

public class Entry
{
    [UnmanagedCallersOnly(CallConvs = [ typeof(CallConvCdecl)], EntryPoint = "RegisterEntryModule")]
    public unsafe static void RegisterEntryModule()
    {
        var moduleName = Marshal.StringToHGlobalAnsi("entry");

        napi_module demoModule = new napi_module
        {
            nm_version = 1,
            nm_flags = 0,
            nm_filename = null,
            nm_modname = (sbyte*)moduleName,
            nm_priv = null,
            napi_addon_register_func = &Init,
            reserved_0 = null,
            reserved_1 = null,
            reserved_2 = null,
            reserved_3 = null,
        };
        ace_napi.napi_module_register(&demoModule);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static napi_value Init(napi_env env, napi_value exports)
    {
        var interceptRequestRequestName = Marshal.StringToHGlobalAnsi("interceptRequest");
        var createBlazorName = Marshal.StringToHGlobalAnsi("createBlazor");
        var messageReceiveName = Marshal.StringToHGlobalAnsi("messageReceive");
        Span<napi_property_descriptor> desc = [
            new (){utf8name = (sbyte*)interceptRequestRequestName, name = default, method = &WebViewInterceptRequest, getter = default, setter = default, value = default,  attributes = napi_property_attributes.napi_default, data = null},
            new (){utf8name = (sbyte*)createBlazorName, name = default, method = &CreateBlazor, getter = default, setter = default, value = default,  attributes = napi_property_attributes.napi_default, data = null},
            new (){utf8name = (sbyte*)messageReceiveName, name = default, method = &MessageReceive, getter = default, setter = default, value = default,  attributes = napi_property_attributes.napi_default, data = null}
        ];
        fixed (napi_property_descriptor* p = desc)
        {
            ace_napi.napi_define_properties(env, exports, (ulong)desc.Length, p);
        }
        return exports;
    }


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static napi_value WebViewInterceptRequest(napi_env env, napi_callback_info info)
    {
        ulong argc = 2;
        napi_value* args = stackalloc napi_value[2];
        if (ace_napi.napi_get_cb_info(env, info, &argc, args, null, null) != napi_status.napi_ok)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "WebViewInterceptRequest get cb info failed");
            return default;
        }

        string? url = null;
        fixed (sbyte* urlData = data)
        {
            ulong urlLength = default;
            if (ace_napi.napi_get_value_string_utf8(env, args[0], urlData, 1024, &urlLength) != napi_status.napi_ok)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "WebViewInterceptRequest url get failed");
                return default;
            }
            url = Marshal.PtrToStringUTF8((nint)urlData);
        }

        if (url == null)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "WebViewInterceptRequest url is null");
            return default;
        }

        if (webview == null)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "WebViewInterceptRequest app not found");
            return default;
        }

        return webview.InterceptRequest(env, url, args[1]);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static napi_value CreateBlazor(napi_env env, napi_callback_info info)
    {
        try
        {
            ulong argc = 2;
            napi_value* args = stackalloc napi_value[2];
            ace_napi.napi_get_cb_info(env, info, &argc, args, null, null);

            webview = App.Create(env, args[0], args[1]);
        } 
        catch (Exception e)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "CreateBlazor Message :" + e.Message);
            if (e.StackTrace != null)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "CreateBlazor StackTrace :" + e.StackTrace);
            }
            if (e.InnerException != null)
            {
                e = e.InnerException;
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "CreateBlazor Message :" + e.Message);
                if (e.StackTrace != null)
                {
                    Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "CreateBlazor StackTrace :" + e.StackTrace);
                }
            }
        }

        return default;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static napi_value MessageReceive(napi_env env, napi_callback_info info)
    {
        ulong argc = 1;
        napi_value* args = stackalloc napi_value[1];
        if (ace_napi.napi_get_cb_info(env, info, &argc, args, null, null) != napi_status.napi_ok)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "MessageReceive get cb info failed");
            return default;
        }

        ulong messageLength = default;
        string? message = null;
        fixed (sbyte* messageData = data)
        {
            if (ace_napi.napi_get_value_string_utf8(env, args[0], messageData, (ulong)data.Length, &messageLength) != napi_status.napi_ok)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "MessageReceive message get failed");
                return default;
            }
            message = Marshal.PtrToStringUTF8((nint)messageData);
        }
        if (message == null)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "MessageReceive message is null");
            return default;
        }

        if (webview == null)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "BlazorHybrid", "MessageReceive app not found");
            return default;
        }

        webview.MessageReceived(message);

        return default;
    }

    static sbyte[] data = new sbyte[1024 * 1024];
    static BlazorWebview? webview;


}
