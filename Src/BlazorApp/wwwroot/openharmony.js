console.log("runJavaScript ok!");
window.port2 = null;

window.__receiveMessageCallbacks = [];

window.external = {
    receiveMessage: function (callback)
    {
        window.__receiveMessageCallbacks.push(callback);
    },
    sendMessage: function (message)
    {
        window.port2.postMessage(message)
    }

}
    

window.addEventListener('message', (e) => {
    if(e.data === '__init_ports__') {
        console.log("receive message init port2");
        window.port2 = e.ports[0]

        window.port2.onmessage = message => {
            console.log("receive message from port2", message.data);
            window.__receiveMessageCallbacks.forEach(callback => callback(message.data));
        }
        Blazor.start();
    }
})