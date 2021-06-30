using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
                    (float)currentDraggingPoint.Y - (float)previousDraggingPoint.Y
                );

            MoveDrawnImage(displacementVector);
        }

        public void MoveDrawnImage(Vector2 displacementVector)
        {
            Point newPoint = new Point(
                    this.Location.X + displacementVector.X,
                    this.Location.Y + displacementVector.Y
                );

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

            CenterDrawnImage();
        }

        public void CenterDrawnImage()
        {
            Point centerPoint = GetCanvasCenterPoint();

            this.Location = new Point(
                    centerPoint.X - this.Size.Width / 2,
                    centerPoint.Y - this.Size.Height / 2
                );

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

            double zoomFactor = zoomPercentage / 100;

            this.Size = new Size(
                    _sourceImage.Size.Width * zoomFactor,
                    _sourceImage.Size.Height * zoomFactor
                );



            Point fixedPointInDrawnImage = PointRelativeToDrawnImage(fixedPoint);

            double deltaX = fixedPointInDrawnImage.X * (zoomFactor - 1);
            double deltaY = fixedPointInDrawnImage.Y * (zoomFactor - 1);

            this.Location = new Point(
                    this.Location.X - deltaX,
                    this.Location.Y - deltaY
                );









            //Point fixedPointInDrawnImage = PointRelativeToDrawnImage(fixedPoint);

            //double deltaX = fixedPointInDrawnImage.X * (zoomFactor - 1);
            //double deltaY = fixedPointInDrawnImage.Y * (zoomFactor - 1);

            //this.Location = new Point(
            //        this.Location.X - deltaX,
            //        this.Location.Y - deltaY
            //    );

            this.ZoomPercentage = zoomPercentage;

            _canvas.Invalidate();
        }

        private Point PointRelativeToDrawnImage(Point pointOnCanvas)
        {
            return new Point(
                    pointOnCanvas.X - this.Location.X,
                    pointOnCanvas.Y - this.Location.Y
                );
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
