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
        static bool _serverInitialized = false;

        static Skeleton[] _skeletons = new Skeleton[6];

        static void Main(string[] args)
        {
            InitilizeKinect();
            InitializeServer();
        }

        private static void InitializeServer()
        {
            var server = new WebSocketServer("ws://localhost:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Connected to " + socket.ConnectionInfo.ClientIpAddress);
                    _clients.Add(socket);
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("Disconnected from " + socket.ConnectionInfo.ClientIpAddress);
                    _clients.Remove(socket);
                };

                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                };
            });

            _serverInitialized = true;

            Console.ReadLine();
        }

        private static void InitilizeKinect()
        {
            var sensor = KinectSensor.KinectSensors.SingleOrDefault();

            if (sensor != null)
            {
                sensor.ColorStream.Enable();
                sensor.SkeletonStream.Enable();

                sensor.AllFramesReady += Sensor_AllFramesReady;

                sensor.Start();
            }
        }

        static void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (!_serverInitialized) return;

            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    var blob = frame.Serialize();

                    foreach (var socket in _clients)
                    {
                        socket.Send(blob);
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
}
