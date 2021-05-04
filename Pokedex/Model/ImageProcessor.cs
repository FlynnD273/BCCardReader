extern alias ShimDrawing;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using ShimDrawing::System.Drawing.Imaging;
using ShimDrawing::System.Drawing;
using Image = AForge.Imaging.Image;



namespace Pokedex.Model
{
    static class ImageProcessor
    {
        private static readonly double _pokemonCardRatio = 63.0 / 88; // Width / Height

        public static Bitmap Format (Bitmap bitmap)
        {
            return Image.Clone(bitmap, PixelFormat.Format24bppRgb);
        }

        public static Bitmap Format8bpp(Bitmap bitmap)
        {
            return Image.Clone(bitmap, PixelFormat.Format8bppIndexed);
        }

        private static Bitmap _Preprocess(Bitmap bitmap)
        {
            Bitmap grayImage = Grayscale.CommonAlgorithms.RMY.Apply(Format(bitmap));

            GaussianBlur blurFilter = new GaussianBlur()
            {
                ProcessAlpha = false,
                Size = 15,
            };

            FiltersSequence filters = new FiltersSequence();
            filters.Add(new Threshold(50));
            filters.Add(blurFilter);
            filters.Add(new CannyEdgeDetector());
            filters.Add(new Dilatation());
            return filters.Apply(grayImage);
        }

        public static Bitmap FindPlayingCard(Bitmap bitmap)
        {
            var b = Format(bitmap);
            return _CropToQuad(b, _GetLargestBlob(_FindQuads(_Preprocess(b))));
        }

        private static Bitmap _CropToQuad(Bitmap bitmap, List<IntPoint> corners)
        {
            if (corners == null)
            {
                return null;
            }

            // create filter
            SimpleQuadrilateralTransformation filter =
                new SimpleQuadrilateralTransformation(corners, (int)(500 * _pokemonCardRatio), 500);

            return filter.Apply(bitmap);
        }

        private static List<List<IntPoint>> _FindQuads(Bitmap bitmap)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinHeight = 32;
            blobCounter.MinWidth = 32;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;

            blobCounter.ProcessImage(bitmap);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            List<List<IntPoint>> foundObjects = new List<List<IntPoint>>();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            List<IntPoint> corners;

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);

                // does it look like a quadrilateral ?
                if (shapeChecker.IsQuadrilateral(edgePoints, out corners))
                {
                    foundObjects.Add(corners);
                }
            }

            return foundObjects;
        }

        private static List<IntPoint> _GetLargestBlob(List<List<IntPoint>> blobs)
        {
            List<(double Area, List<IntPoint> Corners)> shapes = new List<(double Area, List<IntPoint> Corners)>();

            foreach (var blob in blobs)
            {
                shapes.Add((_GetAreaOfBounds(blob), blob));
            }

            shapes.Sort((a, b) => a.Area.CompareTo(b.Area));

            if (shapes.Count == 0)
            {
                return null;
            }
            var c = shapes[0].Corners;

            Size s = _GetBoundingBox(c);

            if (c[0].X > c[2].X || c[0].Y > c[2].Y)
            {
                if (c[0].Y > c[2].Y)
                {
                    c.Add(c[0]);
                    c.RemoveAt(0);
                }
                else
                {
                    c.Insert(0, c[3]);
                    c.RemoveAt(3);
                }
            }
            return c;
        }

        private static double _GetAreaOfBounds(List<IntPoint> points)
        {
            Size s = _GetBoundingBox(points);
            return s.Width * s.Height;
        }

        private static Size _GetBoundingBox (List<IntPoint> points)
        {
            int xMin = points[0].X,
                xMax = points[0].X,
                yMin = points[0].Y,
                yMax = points[0].Y;

            foreach (IntPoint p in points)
            {
                xMin = xMin < p.X ? xMin : p.X;
                xMax = xMax > p.X ? xMax : p.X;
                yMin = yMin < p.Y ? yMin : p.Y;
                yMax = yMax > p.Y ? yMax : p.Y;
            }

            return new Size(xMax - xMin, yMax - yMin);
        }
    }
}
