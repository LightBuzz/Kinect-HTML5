window.onload = function () {
    var status = document.getElementById("status");
    var canvas = document.getElementById("canvas");
    var buttonColor = document.getElementById("color");
    var buttonDepth = document.getElementById("depth");
    var context = canvas.getContext("2d");

    var camera = new Image();

    camera.onload = function () {
        context.drawImage(camera, 0, 0);
    }

    if (!window.WebSocket) {
        status.innerHTML = "Your browser does not support web sockets!";
        return;
    }

    status.innerHTML = "Connecting to server...";

    // Initialize a new web socket.
    var socket = new WebSocket("ws://localhost:8181");
    //socket.binaryType = 'arraybuffer';

    // Connection established.
    socket.onopen = function () {
        status.innerHTML = "Connection successful.";
    };

    // Connection closed.
    socket.onclose = function () {
        status.innerHTML = "Connection closed.";
    }

    // Receive data FROM the server!
    socket.onmessage = function (event) {
        context.clearRect(0, 0, canvas.width, canvas.height);
        if (typeof event.data === "string") {
            // SKELETON DATA

            // Get the data in JSON format.
            var jsonObject = JSON.parse(event.data);

            // Display the skeleton joints.
            for (var i = 0; i < jsonObject.skeletons.length; i++) {
                for (var j = 0; j < jsonObject.skeletons[i].joints.length; j++) {
                    var joint = jsonObject.skeletons[i].joints[j];

                    // Draw!!!
                    context.fillStyle = "#FF0000";
                    context.beginPath();
                    context.arc(joint.x, joint.y, 10, 0, Math.PI * 2, true);
                    context.closePath();
                    context.fill();
                }
            }
        }
        else if (event.data instanceof Blob) {
            // RGB FRAME DATA
            // 1. Get the raw data.
            var blob = event.data;

            // Not supported in modern browsers.
            //window.URL = window.URL || window.webkitURL;
            //var source = window.URL.createObjectURL(blob);
            //camera.src = source;
            //window.URL.revokeObjectURL(source);
        }
    };

    buttonColor.onclick = function () {
        socket.send("Color");

        canvas.width = 1920;
        canvas.height = 1080;
    }

    buttonDepth.onclick = function () {
        socket.send("Depth");

        canvas.width = 512;
        canvas.height = 424;
    }
};