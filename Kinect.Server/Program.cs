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
        private static List<IWebSocketConnection> _clients = new List<IWebSocketConnection>();

        private static Body[] _skeletons = new Body[6];
        private static MultiSourceFrameReader _reader;

        private static Mode _mode = Mode.Color;

        private static CoordinateMapper _coordinateMapper;

        static void Main(string[] args)
        {
            InitializeConnection();
            InitilizeKinect();

            Console.ReadLine();
        }

        private static void InitializeConnection()
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
        }

        private static void InitilizeKinect()
        {
            var sensor = KinectSensor.GetDefault();

            if (sensor != null)
            {
                sensor.Open();

                _coordinateMapper = sensor.CoordinateMapper;
                _reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private static void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multisource = e.FrameReference.AcquireFrame();

            using (var frame = multisource.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        var blob = frame.Serialize();

                        foreach (var socket in _clients)
                        {
                            socket.Send(blob);
                        }
                    }
                }
            }

            using (var frame = multisource.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        var blob = frame.Serialize();

                        foreach (var socket in _clients)
                        {
                            socket.Send(blob);
                        }
                    }
                }
            }

            using (var frame = multisource.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_skeletons);

                    var users = _skeletons.Where(s => s.IsTracked).ToList();

                    if (users.Count > 0)
                    {
                        string json = users.Serialize(_coordinateMapper, _mode);

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
