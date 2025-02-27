import { webview} from '@kit.ArkWeb';

export const interceptRequest: (url: string, createResponse: any) => object;

export const createBlazor: (sendMessage: any, navigateCore: any) => void;

export const messageReceive: (message: string) => void;