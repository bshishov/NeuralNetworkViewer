using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Utilities;
using Color = System.Drawing.Color;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(ImageOutputViewModel))]
    class ImageOutputViewModel : Tool, IHandle<NetworkUpdatedMessage>
    {
        private WriteableBitmap _output;
        private Int32Rect rect;
        private byte[] colorArray;
        private int arraySize;
        private int stride;
        private int bytesPerPixel;
        private double[] inputs = new double[2];
        private const int Width = 128;
        private const int Height = 128;

        public WriteableBitmap Output
        {
            get { return _output; }
            set
            {
                _output = value;
                NotifyOfPropertyChange(() => Output);
            }
        }

        [ImportingConstructor]
        public ImageOutputViewModel(IEventAggregator events)
        {
            DisplayName = "Image Output";
            events.Subscribe(this);
            _output = new WriteableBitmap(Width, Height, 96.0, 96.0, PixelFormats.Bgra32, null);
            bytesPerPixel = (_output.Format.BitsPerPixel + 7) / 8;
            stride = _output.PixelWidth * bytesPerPixel;
            arraySize = stride * _output.PixelHeight;
            colorArray = new byte[arraySize];
            rect = new Int32Rect(0, 0, _output.PixelWidth, _output.PixelHeight);
        }

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType != NetworkUpdateType.SmallChanges) return;
           
            var network = message.Network;

            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
            {
                inputs[0] = (i / (double)_output.Width) * 2.0 - 1.0;
                inputs[1] = (j / (double)_output.Height) * 2.0 - 1.0;
                var result = network.Calculate(inputs);

                colorArray[(i * Height + j) * bytesPerPixel + 0] = (byte)Clamp(result[0], 0, 255);
                colorArray[(i * Height + j) * bytesPerPixel + 1] = (byte)Clamp(result[1], 0, 255);
                colorArray[(i * Height + j) * bytesPerPixel + 2] = (byte)Clamp(result[2], 0, 255);
                colorArray[(i * Height + j) * bytesPerPixel + 3] = 255;
            }
            _output.WritePixels(rect, colorArray, stride, 0);
        }

        private static double Clamp(double val, double min, double max)
        {
            return Math.Min(max, Math.Max(val, min));
        }
    }
}
