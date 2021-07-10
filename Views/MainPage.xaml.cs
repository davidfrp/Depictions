using Depictions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Depictions
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool AllowPanning { get; set; } = true;

        public bool AllowMouseWheelZoom { get; set; } = true;

        private DepictionSizeMode _sizeMode;
        public DepictionSizeMode SizeMode
        {
            get => _sizeMode;
            set
            {
                _sizeMode = value;
                Image image = new Image();

                switch (_sizeMode)
                {
                    case DepictionSizeMode.None:
                        _depiction?.RevertRenderedImage();
                        break;
                    case DepictionSizeMode.Fit:
                        _depiction?.FillRenderedImageInCanvas(false, false);
                        break;
                    case DepictionSizeMode.Fill:
                        _depiction?.FillRenderedImageInCanvas(true, false);
                        break;
                    case DepictionSizeMode.Center:
                        _depiction?.CenterRenderedImage();
                        break;
                }
            }
        }

        public CanvasBitmap SourceImage { get; set; }

        private Depiction _depiction;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpe");
            fileOpenPicker.FileTypeFilter.Add(".jfif");
            fileOpenPicker.FileTypeFilter.Add(".png");

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            //StorageFolder storageFolder = KnownFolders.MusicLibrary;

            if (file != null)
                await depictionControl.LoadSourceImageAsync(file);

            //MessageDialog messageDialog = new MessageDialog($"{this.SourceImage?.Bounds.Width}x{this.SourceImage?.Bounds.Height}", "Image properties");
            //messageDialog.ShowAsync();
        }

        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (this.SourceImage != null)
            {
                CanvasImageInterpolation interpolationMode = CanvasImageInterpolation.HighQualityCubic;

                if (_isMovingImage)
                    interpolationMode = CanvasImageInterpolation.Linear;

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

        private async Task LoadSourceImageAsync(StorageFile file)
        {
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                this.SourceImage = await CanvasBitmap.LoadAsync(canvas, stream);

                _depiction = new Depiction(canvas, this.SourceImage);

                this.SizeMode = _sizeMode;
            }

            canvas.Invalidate();
        }

        private Point _previousDraggingPoint;
        private bool _isMovingImage;

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((CanvasControl)sender).CapturePointer(e.Pointer);

            _previousDraggingPoint = e.GetCurrentPoint(sender as UIElement).Position;

            _isMovingImage = true;
        }

        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((CanvasControl)sender).ReleasePointerCapture(e.Pointer);

            _isMovingImage = false;

            canvas.Invalidate();
        }

        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point currentDraggingPoint = e.GetCurrentPoint(sender as UIElement).Position;

            if (_isMovingImage)
                _depiction.MoveRenderedImage(_previousDraggingPoint, currentDraggingPoint);

            _previousDraggingPoint = currentDraggingPoint;
        }

        private void canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (!AllowMouseWheelZoom)
                return;

            PointerPoint currentPointerPoint = e.GetCurrentPoint(sender as UIElement);

            // Determine whether the user zooms in or out, depending on the wheel direction.
            bool isZoomingIn = currentPointerPoint.Properties.MouseWheelDelta > 0;

            // Depending on zoom direction, increase or decrease current zoom by 10%
            _depiction?.ZoomRenderedImage(_depiction.ZoomPercentage * (isZoomingIn ? 1.1 : 0.9), currentPointerPoint.Position);
        }

        private void canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_depiction == null)
                return;

            // Fit
            // Fill
            // 1:1

            if (this.SizeMode == DepictionSizeMode.Fit)
            {
                _sizeMode = DepictionSizeMode.Fill;

                _depiction.FillRenderedImageInCanvas(true, false, e.GetPosition(canvas));
            }
            else if (_depiction.ZoomPercentage == 100)
            {
                this.SizeMode = DepictionSizeMode.Fit;
            }
            else
            {
                _depiction.ZoomRenderedImage(100, e.GetPosition(canvas));

                if (_depiction.RenderSize.Width < canvas.Size.Width &&
                    _depiction.RenderSize.Height < canvas.Size.Height)
                    _depiction.CenterRenderedImage();
            }
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // TODO: Improve performance while resizing the window.
            this.SizeMode = _sizeMode;
        }
    }
}
