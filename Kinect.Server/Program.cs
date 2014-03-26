using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fleck;
using Microsoft.Kinect;

namespace Kinect.Server
{
    class Program
    {
        static List<IWebSocketConnection> _clients = new List<IWebSocketConnection>();

        static Skeleton[] _skeletons = new Skeleton[6];

        static Mode _mode = Mode.Color;

        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://localhost:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _clients.Add(socket);
                };

                socket.OnClose = () =>
                {
                    _clients.Remove(socket);
                };

                socket.OnMessage = message =>
                {
                    switch (message)
                    {
                        case "Color":
                            _mode = Mode.Color;
                            break;
                        case "Depth":
                            _mode = Mode.Depth;
                            break;
                        default:
                            break;
                    }

                    Console.WriteLine("Switched to " + message);
                };
            });

            InitilizeKinect();

            Console.ReadLine();
        }

        private static void InitilizeKinect()
        {
            var sensor = KinectSensor.KinectSensors.SingleOrDefault();

            if (sensor != null)
            {
                sensor.ColorStream.Enable();
                sensor.DepthStream.Enable();
                sensor.SkeletonStream.Enable();

                sensor.AllFramesReady += Sensor_AllFramesReady;

                sensor.Start();
            }
        }

        static void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    var blob = frame.Serialize();

                    if (_mode == Mode.Color)
                    {
                        foreach (var socket in _clients)
                        {
                            socket.Send(blob);
                        }
                    }
                }
            }

            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    var blob = frame.Serialize();

                    if (_mode == Mode.Depth)
                    {
                        foreach (var socket in _clients)
                        {
                            socket.Send(blob);
                        }
                    }
                }
            }

            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(_skeletons);

                    var users = _skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).ToList();

                    if (users.Count > 0)
                    {
                        string json = users.Serialize();

                        foreach (var socket in _clients)
                        {
                            socket.Send(json);
                        }
                    }
                }
            }
        }
    }

    enum Mode
    {
        Color,
        Depth
    }
}
