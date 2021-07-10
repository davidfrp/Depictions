using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Depictions
{
    public class Depiction
    {
        public double ZoomPercentage { get; private set; }

        public double MaxZoomPercentage { get; } = 7500;

        // TODO: Clamp location by "boxing" it in.
        private Point _renderLocation;
        public Point RenderLocation 
        { 
            get => _renderLocation;
            set
            {
                _renderLocation = ClampRenderLocation(value);
            }
        }

        // TODO: Clamp size.
        private Size _renderSize;
        public Size RenderSize { get => _renderSize; }

        private CanvasControl _canvas;
        private CanvasBitmap _sourceImage;

        public Depiction(CanvasControl canvas, CanvasBitmap sourceImage)
        {
            _canvas = canvas;
            _sourceImage = sourceImage;

            _renderSize = _sourceImage.Size;
            this.ZoomPercentage = 100;
        }


        public void MoveRenderedImage(Point previousDraggingPoint, Point currentDraggingPoint)
        {
            Vector2 displacementVector = new Vector2(
                (float)currentDraggingPoint.X - (float)previousDraggingPoint.X,
                (float)currentDraggingPoint.Y - (float)previousDraggingPoint.Y);

            MoveRenderedImage(displacementVector);
        }

        public void MoveRenderedImage(Vector2 displacementVector)
        {
            Point newPoint = new Point(
                this.RenderLocation.X + displacementVector.X,
                this.RenderLocation.Y + displacementVector.Y);

            MoveRenderedImage(newPoint);
        }

        public void MoveRenderedImage(Point pointOnCanvas)
        {
            this.RenderLocation = pointOnCanvas;

            _canvas.Invalidate();
        }

        public void RevertRenderedImage()
        {
            this.RenderLocation = new Point();
            _renderSize = _sourceImage.Size;

            _canvas.Invalidate();
        }

        public void FillRenderedImageInCanvas(bool allowOverflow, bool useSourceImageAsMaxSize)
        {
            FillRenderedImageInCanvas(allowOverflow, useSourceImageAsMaxSize, GetCanvasCenterPoint());
        }

        public void FillRenderedImageInCanvas(bool allowOverflow, bool useSourceImageAsMaxSize, Point fixedPoint)
        {
            double widthZoomFactor = _canvas.Size.Width / _sourceImage.Size.Width;
            double heightZoomFactor = _canvas.Size.Height / _sourceImage.Size.Height;

            bool isPortrait = widthZoomFactor > heightZoomFactor;

            double zoomFactor = isPortrait ? widthZoomFactor : heightZoomFactor;

            if (allowOverflow)
                zoomFactor = isPortrait ? heightZoomFactor : widthZoomFactor;

            // Ensure image doesn't get scaled beyond its source's size.
            if (useSourceImageAsMaxSize && zoomFactor > 1)
                zoomFactor = 1;

            ZoomRenderedImage(zoomFactor * 100, fixedPoint);

            Point desiredRenderLocation = _renderLocation;

            CenterRenderedImage();

            if (isPortrait)
            {
                _renderLocation.Y = desiredRenderLocation.Y;

                if (desiredRenderLocation.Y > 0)
                    _renderLocation.Y = 0;

                if (desiredRenderLocation.Y < _canvas.Size.Height - _renderSize.Height)
                    _renderLocation.Y = _canvas.Size.Height - _renderSize.Height;
            }
            else
            {
                _renderLocation.X = desiredRenderLocation.X;

                if (desiredRenderLocation.X > 0)
                    _renderLocation.X = 0;

                if (desiredRenderLocation.X < _canvas.Size.Width - _renderSize.Width)
                    _renderLocation.X = _canvas.Size.Width - _renderSize.Width;
            }
        }

        public void CenterRenderedImage()
        {
            Point centerPoint = GetCanvasCenterPoint();

            this.RenderLocation = new Point(
                centerPoint.X - this.RenderSize.Width / 2,
                centerPoint.Y - this.RenderSize.Height / 2);

            _canvas.Invalidate();
        }

        public Point GetCanvasCenterPoint()
        {
            return new Point(_canvas.Size.Width / 2, _canvas.Size.Height / 2);
        }

        public void ZoomRenderedImage(double zoomPercentage)
        {
            ZoomRenderedImage(zoomPercentage, GetCanvasCenterPoint());
        }

        public void ZoomRenderedImage(double zoomPercentage, Point fixedPoint)
        {
            if (zoomPercentage <= 0)
                return;

            bool isZoomingIn = zoomPercentage > this.ZoomPercentage;

            if ((this.RenderSize.Width <= 64 ||
                this.RenderSize.Height <= 64) &&
                !isZoomingIn)
                return;

            if (zoomPercentage >= this.MaxZoomPercentage &&
                isZoomingIn)
                return;

            // Get the percentage in- or decrease of the source image, as a factor.
            double absoluteScalingFactor = zoomPercentage / 100;

            // Scale the drawn image to the size of the source image multiplied with the absolute factor.
            _renderSize = new Size(
                _sourceImage.Size.Width * absoluteScalingFactor,
                _sourceImage.Size.Height * absoluteScalingFactor);

            // Translate the fixed point to its location on the drawn image.
            Point fixedPointOnRenderedImage = PointRelativeToRenderedImage(fixedPoint);

            // Get the percentage in- or decrease from the drawn image's current zoom level, as a factor.
            double relativeScalingFactor = 1 - (this.ZoomPercentage - zoomPercentage) / this.ZoomPercentage;

            // Calculates the difference in pixel distance from the location.
            double deltaX = fixedPointOnRenderedImage.X * relativeScalingFactor - fixedPointOnRenderedImage.X;
            double deltaY = fixedPointOnRenderedImage.Y * relativeScalingFactor - fixedPointOnRenderedImage.Y;

            // Sets the location of the drawn image.
            this.RenderLocation = new Point(
                this.RenderLocation.X - deltaX,
                this.RenderLocation.Y - deltaY);

            this.ZoomPercentage = zoomPercentage;

            _canvas.Invalidate();
        }

        private Point PointRelativeToRenderedImage(Point pointOnCanvas)
        {
            return new Point(
                pointOnCanvas.X - this.RenderLocation.X,
                pointOnCanvas.Y - this.RenderLocation.Y);
        }

        private Point PointRelativeToSourceImage(Point pointOnCanvas)
        {
            double widthScaleFactor = this.RenderSize.Width / _sourceImage.Size.Width;
            double heightScaleFactor = this.RenderSize.Height / _sourceImage.Size.Height;

            Point pointOnRenderedImage = PointRelativeToRenderedImage(pointOnCanvas);

            double imagePositionX = pointOnRenderedImage.X / widthScaleFactor;
            double imagePositionY = pointOnRenderedImage.Y / heightScaleFactor;

            return new Point(imagePositionX, imagePositionY);
        }

        public Rect SourceImageVisibleRectangle()
        {
            throw new NotImplementedException();
        }

        public Point ClampRenderLocation(Point desiredLocation)
        {
            // Check whether the image is wider than the control.
            if (this.RenderSize.Width >= _canvas.Size.Width)
            {
                // Check whether the image's position is beyond the center of the control.
                if (desiredLocation.X > _canvas.Size.Width / 2)
                {
                    // Constrain the image's position to its positive value.
                    desiredLocation.X = _canvas.Size.Width / 2;
                }
                else if (desiredLocation.X < -(this.RenderSize.Width - _canvas.Size.Width / 2))
                {
                    // Constrain the image's position to its negative value.
                    desiredLocation.X = -(this.RenderSize.Width - _canvas.Size.Width / 2);
                }
            }
            else
            {
                // Check whether the image's center is beyond the right side.
                if (desiredLocation.X + this.RenderSize.Width / 2 > _canvas.Size.Width)
                {
                    // Constrain the image's X-position to prevent going beyond the center of the image.
                    desiredLocation.X = _canvas.Size.Width - this.RenderSize.Width / 2;
                }
                // Check whether the image's center is beyond the left side.
                else if (desiredLocation.X < -(this.RenderSize.Width / 2))
                {
                    // Constrain the image's X-position to prevent going beyond the center of the image.
                    desiredLocation.X = -(this.RenderSize.Width / 2);
                }
            }

            // Check whether the image is higher than the control.
            if (this.RenderSize.Height >= _canvas.Size.Height)
            {
                // Check whether the image's position is beyond the center of the control.
                if (desiredLocation.Y > _canvas.Size.Height / 2)
                {
                    // Constrain the image's position to its positive value.
                    desiredLocation.Y = _canvas.Size.Height / 2;
                }
                else if (desiredLocation.Y < -(this.RenderSize.Height - _canvas.Size.Height / 2))
                {
                    // Constrain the image's position to its negative value.
                    desiredLocation.Y = -(this.RenderSize.Height - _canvas.Size.Height / 2);
                }
            }
            else
            {
                // Check whether the image's center is beyond the top.
                if (desiredLocation.Y + this.RenderSize.Height / 2 > _canvas.Size.Height)
                {
                    // Constrain the image's Y-position to prevent going beyond the center of the image.
                    desiredLocation.Y = _canvas.Size.Height - this.RenderSize.Height / 2;
                }
                // Check whether the image's center is beyond the bottom.
                else if (desiredLocation.Y < -(this.RenderSize.Height / 2))
                {
                    // Constrain the image's Y-position to prevent going beyond the center of the image.
                    desiredLocation.Y = -(this.RenderSize.Height / 2);
                }
            }

            return desiredLocation;
        }
    }
}
