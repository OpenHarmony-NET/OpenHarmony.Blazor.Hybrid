import { webview } from '@kit.ArkWeb';
import hilog from '@ohos.hilog';
import fs from '@ohos.file.fs';
import  entry from 'libentry.so';
import { buffer } from '@kit.ArkTS';
import { displaySync } from '@kit.ArkGraphics2D';

webview.once("webInited", () => {
})

@Entry
@Component
struct Index {
  @State message: string = 'Hello World';
  controller: webview.WebviewController = new webview.WebviewController();
  ports: webview.WebMessagePort[] = [];

  scheme: string = "https";
  baseUri: string = `${this.scheme}://localhost/`;


  aboutToAppear(): void {
    webview.WebviewController.setWebDebuggingAccess(true);

    let backDisplaySync = displaySync.create();
    let range : ExpectedFrameRateRange = {
      expected: 10,
      min:0,
      max:120
    };

    backDisplaySync?.setExpectedFrameRateRange(range)
    backDisplaySync.on("frame", (frameInfo: displaySync.IntervalInfo) => {
      entry.onFrame()
      // hilog.debug(0x0001, "BlazorHybrid", `frame call`);
    });

    backDisplaySync?.start()
  }

  build() {
    Column(){
      Row(){
        Button("hello").onClick(event => {
          this.controller.loadUrl(this.baseUri);
        })
      }
      Row(){
        Web({
          controller: this.controller,
          src: 'https://www.baidu.com'
        }).onAppear(()=>
        {
          entry.createBlazor((message: string) => {
            this.ports[0].postMessageEvent(message)
          },
            (url: string) => {
              this.controller.loadUrl(url)
            });
        }).onPageEnd(() => {
          this.ports = this.controller.createWebMessagePorts();
          this.ports[0].onMessageEvent(message => {
            entry.messageReceive(message.toString())
          })
            this.controller.postMessage('__init_ports__', [this.ports[1]], '*');
        }).onInterceptRequest(event => {
          let requestUrl = event.request.getRequestUrl();
          if (requestUrl.startsWith(this.baseUri))
          {
            if (requestUrl == this.baseUri)
            {
              requestUrl = `${this.baseUri}index.html`
            }
            let response = entry.interceptRequest(requestUrl, (code: number, message: string, mimeType: string, buffer: ArrayBuffer) : WebResourceResponse => {
              hilog.debug(0x0001, "BlazorHybrid", `TS: code: ${code}, message: ${message}, mimeType: ${mimeType}, ArrayBuffer len: ${buffer.byteLength}`);
              let responseWeb = new WebResourceResponse();
              responseWeb.setResponseData(buffer)
              responseWeb.setReasonMessage(message);
              responseWeb.setResponseMimeType(mimeType);
              responseWeb.setResponseCode(code);
              responseWeb.setResponseEncoding('utf-8')
              responseWeb.setResponseIsReady(true)
              return responseWeb;
            });
            return response as WebResourceResponse;
          }
          return null;
        });
      }

    }

  }

}