using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Depictions
{
    /// <summary>
    /// XAML control facilitating the depiction of an image, 
    /// while providing additional features such as panning and zoom.
    /// </summary>
    public sealed class DepictionViewer : Control
    {
        /// <summary>
        /// Whether the image can be zoomed by using the mouse wheel.
        /// </summary>
        public bool AllowMouseWheelZoom { get; set; }

        /// <summary>
        /// Whether double tapping the image, will change size mode.
        /// </summary>
        public bool AllowDoubleTapZoom { get; set; }

        /// <summary>
        /// Whether the image can be panned.
        /// </summary>
        public bool AllowPanning { get; set; }

        private CanvasBitmap _sourceImage;
        private CanvasControl _canvas;

        /// <summary>
        /// Initializes a new instance of the ImageViewer class.
        /// </summary>
        public DepictionViewer()
        {
            this.DefaultStyleKey = typeof(DepictionViewer);

            _canvas = new CanvasControl();
            _canvas.CreateResources += OnCreateResources;
            _canvas.Draw += OnDraw;
        }

        private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the image that is displayed, from an image file.
        /// </summary>
        /// <param name="file">The image file to load.</param>
        public async Task LoadSourceImageAsync(StorageFile file)
        {
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                await LoadSourceImageAsync(stream);
            }
        }

        /// <summary>
        /// Loads the image that is displayed, from a stream.
        /// </summary>
        /// <param name="stream">The stream to load.</param>
        public async Task LoadSourceImageAsync(IRandomAccessStream stream)
        {
            _sourceImage = await CanvasBitmap.LoadAsync(_canvas, stream);

            _canvas.Invalidate();
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_sourceImage == null)
                return;

            args.DrawingSession.DrawImage(_sourceImage);
        }
    }
}
