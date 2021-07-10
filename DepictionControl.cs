using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Depictions
{
    public sealed class DepictionControl : Control
    {
        public bool AllowMouseWheelZoom { get; set; }

        public bool AllowPanning { get; set; }

        public bool AllowDoubleTapZoom { get; set; }

        public DepictionSizeMode SizeMode { get; set; }

        public CanvasBitmap SourceImage { get; set; }

        private bool _isPanning;

        private Point _previousDraggingPoint;

        private Depiction _depiction;
        private CanvasControl _canvas;

        public DepictionControl()
        {
            this.DefaultStyleKey = typeof(DepictionControl);

            _canvas = new CanvasControl();
            _canvas.Draw += OnDraw;

            this.PointerWheelChanged += OnPointerWheelChanged;
            this.DoubleTapped += OnDoubleTapped;
            this.PointerPressed += OnPointerPressed;
            this.PointerMoved += OnPointerMoved;
            this.PointerReleased += OnPointerReleased;
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (this.SourceImage != null)
            {
                CanvasImageInterpolation interpolationMode = CanvasImageInterpolation.HighQualityCubic;

                //if (_isMovingImage)
                //    interpolationMode = CanvasImageInterpolation.Linear;

                if (_depiction.ZoomPercentage >= 100)
                    interpolationMode = CanvasImageInterpolation.NearestNeighbor;

                Rect destinationRectangle = new Rect(
                    _depiction.RenderLocation.X,
                    _depiction.RenderLocation.Y,
                    _depiction.RenderSize.Width,
                    _depiction.RenderSize.Height);

                Rect sourceRectangle = new Rect(0, 0,
                    this.SourceImage.Size.Width,
                    this.SourceImage.Size.Height);

                args.DrawingSession.DrawImage(
                    this.SourceImage,
                    destinationRectangle,
                    sourceRectangle, 1,
                    interpolationMode);
            }
        }

        public async Task LoadSourceImageAsync(StorageFile file)
        {
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                this.SourceImage = await CanvasBitmap.LoadAsync(_canvas, stream);

                _depiction = new Depictions.Depiction(_canvas, this.SourceImage);

                // this.SizeMode = _sizeMode;
            }

            _canvas.Invalidate();
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (this.AllowMouseWheelZoom)
            {
                PointerPoint currentPointerPoint = e.GetCurrentPoint(_canvas);

                // Determine whether the user zooms in or out, depending on the wheel direction.
                bool isZoomingIn = currentPointerPoint.Properties.MouseWheelDelta > 0;

                // Depending on zoom direction, increase or decrease current zoom by 10%
                _depiction?.ZoomRenderedImage(_depiction.ZoomPercentage * (isZoomingIn ? 1.1 : 0.9), currentPointerPoint.Position);
            }
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.AllowDoubleTapZoom)
            {
                if (this.SizeMode == DepictionSizeMode.Fit)
                {
                    // _sizeMode = DepictionSizeMode.Fill;

                    // FIXME

                    _depiction.FillRenderedImageInCanvas(true, false, e.GetPosition(_canvas));
                }
                else if (_depiction.ZoomPercentage == 100)
                {
                    this.SizeMode = DepictionSizeMode.Fit;
                }
                else
                {
                    _depiction.ZoomRenderedImage(100, e.GetPosition(_canvas));

                    if (_depiction.RenderSize.Width < _canvas.Size.Width &&
                        _depiction.RenderSize.Height < _canvas.Size.Height)
                        _depiction.CenterRenderedImage();
                }
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isPanning = _canvas.CapturePointer(e.Pointer);

            _previousDraggingPoint = e.GetCurrentPoint(_canvas).Position;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.AllowPanning && _isPanning)
            {
                Point currentDraggingPosition = e.GetCurrentPoint(_canvas).Position;

                _depiction.MoveRenderedImage(_previousDraggingPoint, currentDraggingPosition);
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _canvas.ReleasePointerCapture(e.Pointer);

            _isPanning = false;

            _canvas.Invalidate();
        }
    }
}
