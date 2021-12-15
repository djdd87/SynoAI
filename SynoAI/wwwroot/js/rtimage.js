// euquiq: In charge of receiving the snapshot from SynoAI SignalR's Hub and populating the <img>

"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/synoaiHub")
    .build();

connection.on("ReceiveSnapshot", (cameraName, fileName) => {
    var image = document.getElementById("camera-" + cameraName);
    if (image != null) {
        image.alt = fileName;
        image.src = "/" + cameraName + "/" + fileName;
    }
});

connection.onclose(async () => {
    await start();
});

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

// Start the connection.
start();