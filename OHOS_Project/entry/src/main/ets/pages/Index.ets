import { webview } from '@kit.ArkWeb';
import hilog from '@ohos.hilog';
import  entry from 'libentry.so';

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
          return entry.dotnetInterceptRequest(this.controller, requestUrl);
        }
        else if (requestUrl.startsWith(this.baseUri))
        {
          let responseWeb = new WebResourceResponse();
          let path = requestUrl.replace(this.baseUri, 'wwwroot/');
          if (path == 'wwwroot/')
          {
            path = "wwwroot/index.html";
          }
          let file = $rawfile(path);
          responseWeb.setResponseData($rawfile(path))
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
          responseWeb.setReasonMessage("OK");
          responseWeb.setResponseIsReady(true)
          return responseWeb;
        }
        return null;
      });
    }
  }

}