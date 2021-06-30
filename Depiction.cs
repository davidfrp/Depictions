using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.Foundation;

namespace DepictionsApp
{
    internal class Depiction
    {
        public double ZoomPercentage { get; private set; }

        // TODO: Clamp location by "boxing" it in.
        public Point Location { get; private set; }

        // TODO: Clamp size.
        public Size Size { get; private set; }

        private CanvasControl _canvas;
        private CanvasBitmap _sourceImage;

        public Depiction(CanvasControl canvas, CanvasBitmap sourceImage)
        {
            _canvas = canvas;
            _sourceImage = sourceImage;

            this.Size = _sourceImage.Size;
            this.ZoomPercentage = 100;
        }

        public void MoveDrawnImage(Point previousDraggingPoint, Point currentDraggingPoint)
        {
            Vector2 displacementVector = new Vector2(
                (float)currentDraggingPoint.X - (float)previousDraggingPoint.X,
                (float)currentDraggingPoint.Y - (float)previousDraggingPoint.Y);

            MoveDrawnImage(displacementVector);
        }

        public void MoveDrawnImage(Vector2 displacementVector)
        {
            Point newPoint = new Point(
                this.Location.X + displacementVector.X,
                this.Location.Y + displacementVector.Y);

            MoveDrawnImage(newPoint);
        }

        public void MoveDrawnImage(Point pointOnCanvas)
        {
            this.Location = pointOnCanvas;

            _canvas.Invalidate();
        }

        public void RevertDrawnImage()
        {
            this.Location = new Point();
            this.Size = _sourceImage.Size;

            _canvas.Invalidate();
        }

        public void FillDrawnImageInCanvas(bool allowOverflow, bool useSourceImageAsMaxSize)
        {
            double widthZoomFactor = _canvas.Size.Width / _sourceImage.Size.Width;
            double heightZoomFactor = _canvas.Size.Height / _sourceImage.Size.Height;

            double zoomFactor = widthZoomFactor < heightZoomFactor ? widthZoomFactor : heightZoomFactor;

            if (allowOverflow)
                zoomFactor = widthZoomFactor > heightZoomFactor ? widthZoomFactor : heightZoomFactor;

            // Ensure image doesn't get scaled beyond its source's size.
            if (useSourceImageAsMaxSize && zoomFactor > 1)
                zoomFactor = 1;

            ZoomDrawnImage(zoomFactor * 100);

            // TODO: Remove this line.
            CenterDrawnImage();
        }

        public void CenterDrawnImage()
        {
            Point centerPoint = GetCanvasCenterPoint();

            this.Location = new Point(
                centerPoint.X - this.Size.Width / 2,
                centerPoint.Y - this.Size.Height / 2);

            _canvas.Invalidate();
        }

        public Point GetCanvasCenterPoint()
        {
            return new Point(_canvas.Size.Width / 2, _canvas.Size.Height / 2);
        }

        public void ZoomDrawnImage(double zoomPercentage)
        {
            ZoomDrawnImage(zoomPercentage, GetCanvasCenterPoint());
        }

        public void ZoomDrawnImage(double zoomPercentage, Point fixedPoint)
        {
            if (zoomPercentage <= 0)
                return;

            bool isZoomingIn = zoomPercentage > this.ZoomPercentage;

            if ((this.Size.Width < 64 ||
                this.Size.Height < 64) &&
                !isZoomingIn)
                return;

            if (zoomPercentage > 7500 &&
                isZoomingIn)
                return;

            // Get the percentage in- or decrease of the source image, as a factor.
            double absoluteScalingFactor = zoomPercentage / 100;

            // Scale the drawn image to the size of the source image multiplied with the absolute factor.
            this.Size = new Size(
                _sourceImage.Size.Width * absoluteScalingFactor,
                _sourceImage.Size.Height * absoluteScalingFactor);

            // Translate the fixed point to its location on the drawn image.
            Point fixedPointOnDrawnImage = PointRelativeToDrawnImage(fixedPoint);

            // Get the percentage in- or decrease from the drawn image's current zoom level, as a factor.
            double relativeScalingFactor = 1 - (this.ZoomPercentage - zoomPercentage) / this.ZoomPercentage;

            // Calculates the difference in pixel distance from the location.
            double deltaX = fixedPointOnDrawnImage.X * relativeScalingFactor - fixedPointOnDrawnImage.X;
            double deltaY = fixedPointOnDrawnImage.Y * relativeScalingFactor - fixedPointOnDrawnImage.Y;

            // Sets the location of the drawn image.
            this.Location = new Point(
                this.Location.X - deltaX,
                this.Location.Y - deltaY);

            this.ZoomPercentage = zoomPercentage;

            _canvas.Invalidate();
        }

        private Point PointRelativeToDrawnImage(Point pointOnCanvas)
        {
            return new Point(
                pointOnCanvas.X - this.Location.X,
                pointOnCanvas.Y - this.Location.Y);
        }

        private Point PointRelativeToSourceImage(Point pointOnCanvas)
        {
            double widthScaleFactor = this.Size.Width / _sourceImage.Size.Width;
            double heightScaleFactor = this.Size.Height / _sourceImage.Size.Height;

            Point pointOnDrawnImage = PointRelativeToDrawnImage(pointOnCanvas);

            double imagePositionX = pointOnDrawnImage.X / widthScaleFactor;
            double imagePositionY = pointOnDrawnImage.Y / heightScaleFactor;

            return new Point(imagePositionX, imagePositionY);
        }

        public Rect SourceImageVisibleRectangle()
        {
            throw new NotImplementedException();
        }
    }
}
