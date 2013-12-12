Kinect-HTML5
============

Display Kinect data on an HTML5 canvas using WebSockets.

Description
---
This project connects a Kinect-enabled application to an HTML5 web page and displays the users' skeletons.

The application acts as a WebSocket server, transmitting new skeleton data whenever Kinect frames are available. The web page uses WebSockets to get the Kinect data and display them on a canvas.

Prerequisites
---
* [Kinect for Windows](http://amzn.to/1k7rquZ) or [Kinect for XBOX](http://amzn.to/1dO0R0s) sensor
* [Kinect for Windows SDK v1.8](http://go.microsoft.com/fwlink/?LinkID=323588)

WebSockets
---
Read more about WebSockets in [Getting Started with HTML5 WebSocket Programming, by Vangos Pterneas](http://amzn.to/19cvMj9).

Credits
---
The WebSocket server application uses [Fleck, by Jason Staten](https://github.com/statianzo/Fleck).

License
---
You are free to use these libraries in personal and commercial projects by attributing the original creator of Vitruvius. Licensed under [Apache v2 License](https://github.com/LightBuzz/Kinect-HTML5/blob/master/LICENSE).
