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
        static readonly string CAPTURE_FILE = "Capture.jpg";

        /// <summary>
        /// The bitmap source.
        /// </summary>
        static WriteableBitmap _bitmap = null;

        /// <summary>
        /// The RGB pixel values.
        /// </summary>
        static byte[] _pixels = null;

        public static byte[] Serialize(this ColorImageFrame frame)
        {
            // Create bitmap.
            var format = PixelFormats.Bgra32;
            int width = frame.Width;
            int height = frame.Height;
            int stride = width * format.BitsPerPixel / 8;

            if (_bitmap == null)
            {
                _pixels = new byte[frame.PixelDataLength];
                _bitmap = new WriteableBitmap(width, height, 96.0, 96.0, format, null);
            }

            frame.CopyPixelDataTo(_pixels);

            _bitmap.WritePixels(new Int32Rect(0, 0, width, height), _pixels, stride, 0);
            
            // Save bitmap.
            BitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(_bitmap as BitmapSource));

            using (var stream = new FileStream(CAPTURE_FILE, FileMode.Create))
            {
                encoder.Save(stream);
            }

            // Convert saved bitmap to blob.
            using (FileStream stream = new FileStream(CAPTURE_FILE, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes((int)stream.Length);
                }
            }
        }
    }
}
