using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kinect.Server
{
    /// <summary>
    /// Converts a Kinect frame into an HTML5 blob.
    /// </summary>
    public static class FrameSerializer
    {
        /// <summary>
        /// Maximmum depth distance.
        /// </summary>
        static readonly float MAX_DEPTH_DISTANCE = 4095;

        /// <summary>
        /// Minimum depth distance.
        /// </summary>
        static readonly float MIN_DEPTH_DISTANCE = 850;

        /// <summary>
        /// Maximum depth distance offset.
        /// </summary>
        static readonly float MAX_DEPTH_DISTANCE_OFFSET = MAX_DEPTH_DISTANCE - MIN_DEPTH_DISTANCE;

        /// <summary>
        /// Default name for temporary color files.
        /// </summary>
        static readonly string CAPTURE_FILE_COLOR = "Capture_Color.jpg";

        /// <summary>
        /// Default name for temporary depth files.
        /// </summary>
        static readonly string CAPTURE_FILE_DEPTH = "Capture_Depth.jpg";

        /// <summary>
        /// The color bitmap source.
        /// </summary>
        static WriteableBitmap _colorBitmap = null;

        /// <summary>
        /// The depth bitmap source.
        /// </summary>
        static WriteableBitmap _depthBitmap = null;

        /// <summary>
        /// The RGB pixel values.
        /// </summary>
        static byte[] _colorPixels = null;

        /// <summary>
        /// The RGB depth values.
        /// </summary>
        static byte[] _depthPixels = null;

        /// <summary>
        /// The actual depth values.
        /// </summary>
        static short[] _depthData = null;

        public static byte[] Serialize(this ColorImageFrame frame)
        {
            // Create bitmap.
            var format = PixelFormats.Bgra32;
            int width = frame.Width;
            int height = frame.Height;
            int stride = width * format.BitsPerPixel / 8;

            if (_colorBitmap == null)
            {
                _colorPixels = new byte[frame.PixelDataLength];
                _colorBitmap = new WriteableBitmap(width, height, 96.0, 96.0, format, null);
            }

            frame.CopyPixelDataTo(_colorPixels);

            _colorBitmap.WritePixels(new Int32Rect(0, 0, width, height), _colorPixels, stride, 0);

            return CreateBlob(_colorBitmap, CAPTURE_FILE_COLOR);
        }

        public static byte[] Serialize(this DepthImageFrame frame)
        {
            // Create bitmap.
            var format = PixelFormats.Bgra32;
            int width = frame.Width;
            int height = frame.Height;
            int stride = width * format.BitsPerPixel / 8;

            if (_depthBitmap == null)
            {
                _depthData = new short[frame.PixelDataLength];
                _depthPixels = new byte[height * width * 4];
                _depthBitmap = new WriteableBitmap(width, height, 96.0, 96.0, format, null);
            }

            frame.CopyPixelDataTo(_depthData);

            for (int depthIndex = 0, colorIndex = 0; depthIndex < _depthData.Length && colorIndex < _depthPixels.Length; depthIndex++, colorIndex += 4)
            {
                // Get the depth value.
                int depth = _depthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // Equal coloring for monochromatic histogram.
                byte intensity = (byte)(255 - (255 * Math.Max(depth - MIN_DEPTH_DISTANCE, 0) / (MAX_DEPTH_DISTANCE_OFFSET)));

                _depthPixels[colorIndex + 0] = intensity;
                _depthPixels[colorIndex + 1] = intensity;
                _depthPixels[colorIndex + 2] = intensity;
            }

            _depthBitmap.WritePixels(new Int32Rect(0, 0, width, height), _depthPixels, stride, 0);

            return CreateBlob(_depthBitmap, CAPTURE_FILE_DEPTH);
        }

        public static byte[] CreateBlob(WriteableBitmap bitmap, string file)
        {
            // Save bitmap.
            BitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap as BitmapSource));

            using (var stream = new FileStream(file, FileMode.Create))
            {
                encoder.Save(stream);
            }

            // Convert saved bitmap to blob.
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes((int)stream.Length);
                }
            }
        }
    }
}
