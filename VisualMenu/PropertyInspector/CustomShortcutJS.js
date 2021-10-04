function openEsemwy() {    
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'event': 'openUrl',
            'payload': {
                'url': 'https://github.com/esemwy/StreamDeckSocket'
            }
        };
        websocket.send(JSON.stringify(json));
    }
}
function openWindamyre() {
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'event': 'openUrl',
            'payload': {
                'url': 'https://github.com/Windamyre/DazStudioStreamDeckPlugins'
            }
        };
        websocket.send(JSON.stringify(json));
    }
}
