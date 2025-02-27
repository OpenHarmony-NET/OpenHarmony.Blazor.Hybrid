console.log("runJavaScript ok!");
window.port2 = null;
window.external.receiveMessage = (fun) => {
    console.log("call window.external.receiveMessage");
    if (window.port2 == null)
    {
        console.log("add receiveMessage event failed, port2 is null");
        return;
    }
    window.port2.onmessage = fun;
}

window.external.sendMessage = (message) =>
{
    console.log("call window.external.sendMessage");
    if (window.port2 == null)
    {
        console.log("sendMessage failed, port2 is null");
        return;
    }
    window.port2.postMessage(message)
}
window.addEventListener('message', (e) => {
    if(e.data === '__init_ports__') {
        console.log("receive message init port2");
        window.port2 = e.ports[0]
        Blazor.start();
    }
})