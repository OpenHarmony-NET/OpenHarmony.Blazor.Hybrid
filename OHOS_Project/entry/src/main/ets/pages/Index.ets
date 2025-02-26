import { webview } from '@kit.ArkWeb';
import hilog from '@ohos.hilog';
import fs from '@ohos.file.fs';
import  entry from 'libentry.so';
import { buffer } from '@kit.ArkTS';

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

  sendMessage(message: string) : void
  {
    this.ports[0].postMessageEvent(message)
  }

  build() {
    Column(){
      Web({
        controller: this.controller,
        src: this.baseUri
        // src: 'https://www.baidu.com'
      }).onPageBegin(() => {
        entry.createBlazor(this.controller)
        if (this.ports.length <= 0) {
          this.ports = this.controller.createWebMessagePorts();
          this.ports[0].onMessageEvent(message => {

          })
        }
      }).onInterceptRequest((event) => {

        const requestUrl = event.request.getRequestUrl();
        if (requestUrl.startsWith(`${this.baseUri}_framework/`))
        {
          hilog.debug(0x0001, "testTag", "call .net");
          return entry.interceptRequest(this.controller, requestUrl);
        }
        else if (requestUrl.startsWith(this.baseUri))
        {
          let responseWeb = new WebResourceResponse();
          let path = requestUrl.replace(this.baseUri, 'wwwroot/');
          if (path == 'wwwroot/')
          {
            path = "wwwroot/index.html";
          }


          let resDir = getContext().resourceDir;
          let filePath = `${resDir}/${path}`;
          if (fs.accessSync(filePath) === true)
          {

            let buffers: buffer.Buffer[] = [];
            let inputStream = fs.createStreamSync(filePath, "r");
            let bufSize = 4096;
            let readSize = 0;
            let buf = new ArrayBuffer(bufSize);
            class Option {
              public offset: number = 0;
              public length: number = bufSize;
            }
            let option = new Option();
            option.offset = readSize;
            let readLen =  inputStream.readSync(buf, option);
            buffers.push(buffer.from(buf.slice(0, readLen)))
            readSize += readLen;
            while (readLen > 0) {
              option.offset = readSize;
              readLen =  inputStream.readSync(buf, option);
              buffers.push(buffer.from(buf.slice(0, readLen)))
              readSize += readLen;
            }
            inputStream.closeSync();
            let finalBuf: ArrayBuffer = buffer.concat(buffers).buffer;

            responseWeb.setResponseData(finalBuf)
            responseWeb.setResponseEncoding('utf-8')
            let parts = path.split(".")
            let fileType = parts.pop() ?? ""
            let mimeType = ""
            switch (fileType)
            {
              case "html":
                mimeType = "text/html"
                break;
              case ".css":
                mimeType = "text/css"
                break;
              case ".js":
                mimeType = "text/javascript"
                break;
            }
            responseWeb.setResponseMimeType(mimeType);
            responseWeb.setResponseCode(200);
          }
          else
          {
            responseWeb.setResponseCode(404);
          }


          responseWeb.setReasonMessage("OK");
          responseWeb.setResponseIsReady(true)
          return responseWeb;
        }

        return null;
      });
    }
  }

}