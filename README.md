# Windows10IoTWebControl

Until ASP.NET Core cannot be hosted under Windows 10 IoT background task we need some other way to control background services through web interfaces. As SignalR is not working yet on Windows 10 IoT Core we cannot also use SignalR clients. The only working option I found was using WebSocket directly.

The solution contains two projects that I hosted on my Raspberry:

1. **WebSocketsWebApp** - web application hosted on Raspberry and run automatically when system starts.
2. **WebSocketsBackgroundTask** - simple Windows 10 IoT Core background task that sends random numbers to web application using WebSocket and reads commands inserted through browser (displayed in debug window).

Getting all this stuff work was a little bit tricky. Here are some support materials that may help those who want to try it out:

* [Running ASP.NET Core 2 applications on Windows 10 IoT Core](http://gunnarpeipman.com/2017/12/aspnet-core-windows-iot/) (Gunnar Peipman)
* [ASP.NET Core: Building chat room using WebSocket](http://gunnarpeipman.com/2017/03/aspnet-core-websocket-chat/) (Gunnar Peipman)
* [Communicating with localhost (loopback)](https://docs.microsoft.com/en-us/windows/iot-core/develop-your-app/loopback) (Windows IoT/Microsoft Docs)

Please consider this solution temporary until SignalR client for ASP.NET Core is usable and works stable on Windows 10 IoT Core.
