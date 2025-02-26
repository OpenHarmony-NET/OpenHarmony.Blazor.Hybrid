using OpenHarmony.NDK.Bindings.Native;
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
        var dotnetInterceptRequestName = Marshal.StringToHGlobalAnsi("dotnetInterceptRequest");
        Span<napi_property_descriptor> desc = [
            new (){utf8name = (sbyte*)dotnetInterceptRequestName, name = default, method = &WebViewInterceptRequest, getter = default, setter = default, value = default,  attributes = napi_property_attributes.napi_default, data = null}
        ];
        fixed (napi_property_descriptor* p = desc)
        {
            ace_napi.napi_define_properties(env, exports, 1, p);
        }
        return exports;
    }


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static napi_value WebViewInterceptRequest(napi_env env, napi_callback_info info)
    {
        ulong argc = 2;
        napi_value* args = stackalloc napi_value[2];
        ace_napi.napi_get_cb_info(env, info, &argc, args, null, null);

        sbyte* urlData = stackalloc sbyte[1024];
        ulong urlLength = default;
        ace_napi.napi_get_value_string_utf8(env, args[1], urlData, 1024, &urlLength);
        var url = Marshal.PtrToStringUTF8((nint)urlData);

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "BlazorHybrid", "WebViewInterceptRequest url :" + url);

        return default;
    }



}
