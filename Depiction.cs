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
        public Point Location { get; set; }

        // TODO: Clamp size.
        public Size Size { get; set; }

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
            this.Location = new Point(
                    this.Location.X + displacementVector.X,
                    this.Location.Y + displacementVector.Y
                );

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
            double widthZoomFactor = _canvas.Width / _sourceImage.Size.Width;
            double heightZoomFactor = _canvas.Height / _sourceImage.Size.Height;

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
            return new Point(_canvas.Width / 2, _canvas.Height / 2);
        }

        public void ZoomDrawnImage(double zoomPercentage)
        {
            ZoomDrawnImage(zoomPercentage, GetCanvasCenterPoint());
        }

        public void ZoomDrawnImage(double zoomPercentage, Point fixedPoint)
        {
            // TODO: Attempt to rewrite zoom functionallity.

            // Place image so the fixed pixel is below the mouse cursor.
            Point fixedPixel = PointFromCanvasToSourceImage(fixedPoint);

            Point newLocation = new Point(
                    0,
                    0
                );

            this.Location = newLocation;



            Size newSize = new Size(
                    zoomPercentage / 100 * this.Size.Width,
                    zoomPercentage / 100 * this.Size.Height
                );

            this.Size = newSize;

            _canvas.Invalidate();
        }

        private Point PointFromCanvasToDrawnImage(Point pointOnCanvas)
        {
            return new Point(
                    pointOnCanvas.X - this.Location.X,
                    pointOnCanvas.Y - this.Location.Y
                );
        }

        private Point PointFromCanvasToSourceImage(Point pointOnCanvas)
        {
            double widthScaleFactor = this.Size.Width / _sourceImage.Size.Width;
            double heightScaleFactor = this.Size.Height / _sourceImage.Size.Height;

            double imagePositionX = PointFromCanvasToDrawnImage(pointOnCanvas).X / widthScaleFactor;
            double imagePositionY = PointFromCanvasToDrawnImage(pointOnCanvas).Y / heightScaleFactor;

            return new Point(imagePositionX, imagePositionY);
        }
    }
}
