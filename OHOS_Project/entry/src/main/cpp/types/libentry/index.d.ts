import { webview} from '@kit.ArkWeb';

export const interceptRequest: (webviewController: webview.WebviewController, url: string) => WebResourceResponse;

export const createBlazor: (webviewController: webview.WebviewController) => void;