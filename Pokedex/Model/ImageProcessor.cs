using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Drawing = System.Drawing;

namespace Pokedex.Model
{
    class ImageProcessor
    {
        Mat img;
        public Scalar lowerB { get; private set; }
        public Scalar upperB { get; private set; }
        Scalar hsvRadius;
        private readonly double cardRatio = 63.0 / 88;

        private Tuple<Mat, Mat, string> ProcessImage(Mat img, bool drawRect, bool sharpenImage)
        {
            var mats = FindCard(img);

            Mat topDown = mats.Item2;

            string c = "";

            if (sharpenImage)
                topDown = Sharpen(topDown, 1, 21);

            int y;
            Bitmap topDownBitmap = topDown.ToBitmap();
            Color startCol = topDownBitmap.GetPixel(topDown.Width / 2, 5);
            int threshold = 50;

            for (y = 0; y < topDown.Height / 2; y++)
            {
                Color col = topDownBitmap.GetPixel(topDown.Width / 2, y);
                if (Math.Abs(col.R - startCol.R) > threshold || Math.Abs(col.G - startCol.G) > threshold || Math.Abs(col.B - startCol.B) > threshold)
                {
                    break;
                }
            }

            Rect roi = new Rect((int)(topDown.Width * 0.24), y, (int)(topDown.Width * 0.4), (int)(topDown.Height * 0.064));

            if (roi.Width > 0 && roi.Height > 0)
                c = RunTextRecog(topDown.SubMat(roi));

            if (c == "")
            {
                c = "Could not read text";
            }

            if (drawRect)
                topDown.Rectangle(roi, new Scalar(0, 0, 0), 2);

            return Tuple.Create(topDown, mats.Item1, c.Trim(' '));
        }

        private Drawing.Size GetMaxSize(Drawing.Size winSize, Drawing.Size imgSize, double xFrac, double yFrac)
        {
            double ratio = (double)imgSize.Width / imgSize.Height;
            Drawing.Size finalSize;
            if (winSize.Width * xFrac / imgSize.Width < winSize.Height * yFrac / imgSize.Height)
            {
                finalSize = new Drawing.Size((int)(winSize.Width * xFrac), (int)(winSize.Width * yFrac / ratio));
            }
            else
            {
                finalSize = new Drawing.Size((int)(winSize.Height * xFrac * ratio), (int)(winSize.Height * yFrac));
            }

            return finalSize;
        }

        private Tuple<Mat, Mat> FindCard(Mat img)
        {
            Mat topDown = new Mat();
            img.CopyTo(topDown);
            Mat binImage = new Mat();
            //Mat pre = new Mat();
            //Cv2.GaussianBlur(img, pre, new OpenCvSharp.Size(21, 21), 0);
            Cv2.InRange(img, lowerB, upperB, binImage);

            OpenCvSharp.Point[][] contours;
            Cv2.FindContours(binImage, out contours, out _, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

            Rect bounds = new Rect(0, 0, 0, 0);
            OpenCvSharp.Point center = new OpenCvSharp.Point(0, 0);
            OpenCvSharp.Point imgCenter = new OpenCvSharp.Point(img.Width / 2, img.Height / 2);
            for (int i = 0; i < contours.Length; i++)
            {
                Rect r = Cv2.BoundingRect(contours[i]);
                OpenCvSharp.Point c = new OpenCvSharp.Point((r.Left + r.Right) / 2, (r.Top + r.Bottom) / 2);
                if (r.Width * r.Height > bounds.Width * bounds.Height)// || Dist(center, imgCenter) > Dist(c, imgCenter))
                {
                    bounds = r;
                    center = c;
                }
            }

            if (bounds.Width * bounds.Height > 0)
            {
                Mat temp = binImage.SubMat(bounds);
                binImage = temp.CopyMakeBorder(bounds.Top, img.Height - bounds.Bottom, bounds.Left, img.Width - bounds.Right, BorderTypes.Isolated, new Scalar(255, 255, 255, 255));
            }

            Cv2.BitwiseNot(binImage, binImage);

            Cv2.FindContours(binImage, out contours, out _, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

            Drawing.Point[] poly = null;
            double maxArea = -1;

            for (int i = 0; i < contours.Length; i++)
            {
                OpenCvSharp.Point[] contour = contours[i];
                OpenCvSharp.Point[] approx = Cv2.ApproxPolyDP(contour, Cv2.ArcLength(contour, true) * 0.05, true);
                Drawing.Point[] points = ToDrawingPointArray(approx);

                if (points.Length == 4)
                {
                    double area = Cv2.ContourArea(contour);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        poly = points;
                    }
                }
            }

            if (poly != null)
            {
                topDown = TransformTopDown(topDown, poly);
            }

            return Tuple.Create(binImage, topDown);
        }

        private Mat Sharpen(Mat img, double weight, int blur)
        {
            //Cv2.Resize(img, img, new OpenCvSharp.Size(img.Size().Width * 3 / 2, img.Size().Height * 3 / 2));
            Cv2.PyrUp(img, img);
            Mat blurred = new Mat();
            Cv2.GaussianBlur(img, blurred, new OpenCvSharp.Size(), blur, blur);
            Cv2.AddWeighted(img, 1 + weight, blurred, -weight, 0, img);
            Cv2.PyrDown(img, img);
            return img;
        }

        private Mat TransformTopDown(Mat baseImg, Drawing.Point[] poly)
        {
            Drawing.Point center = new Drawing.Point(0, 0);
            int x = 0;
            int y = 0;
            foreach (Drawing.Point p in poly)
            {
                x += p.X;
                y += p.Y;
            }
            x /= poly.Length;
            y /= poly.Length;

            poly.ToList().Sort((a, b) => Less(a, b, center) ? 0 : 1);

            Drawing.Point[] temp = new Drawing.Point[4];
            temp[0] = poly[3];
            temp[1] = poly[0];
            temp[2] = poly[1];
            temp[3] = poly[2];

            temp.CopyTo(poly, 0);

            double widthA = Dist(poly[0], poly[1]);
            double widthB = Dist(poly[2], poly[3]);
            double heightA = Dist(poly[0], poly[3]);
            double heightB = Dist(poly[1], poly[2]);

            int width = (int)Math.Max(widthA, widthB);
            int height = (int)Math.Max(heightA, heightB);

            if (width > height)
            {
                temp = new Drawing.Point[4];
                temp[0] = poly[1];
                temp[1] = poly[2];
                temp[2] = poly[3];
                temp[3] = poly[0];

                temp.CopyTo(poly, 0);

                int tempDim = width;
                width = height;
                height = tempDim;
            }

            Point2f[] dst = new Point2f[4];
            dst[0] = new Point2f(0, 0);
            dst[1] = new Point2f(width - 1, 0);
            dst[2] = new Point2f(width - 1, height - 1);
            dst[3] = new Point2f(0, height - 1);

            Mat transform = Cv2.GetPerspectiveTransform(ToCvPointArray(poly), dst);

            Mat r = new Mat();
            baseImg.CopyTo(r);
            Cv2.WarpPerspective(r, r, transform, img.Size());

            try
            {
                r = r.SubMat(0, Math.Min(height - 1, r.Cols - 2), 0, Math.Min(width - 1, r.Rows - 2));
            }
            catch (Exception exception)
            {
                r = baseImg;
            }

            Cv2.Resize(r, r, new OpenCvSharp.Size(512, 512 / cardRatio));
            Cv2.Flip(r, r, FlipMode.Y);

            return r;
        }

        private Point2f[] ToCvPointArray(Drawing.Point[] points)
        {
            Point2f[] cv = new Point2f[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Drawing.Point p = points[i];
                cv[i] = new OpenCvSharp.Point(p.X, p.Y);
            }

            return cv;
        }

        private Drawing.Point[] ToDrawingPointArray(OpenCvSharp.Point[] points)
        {
            Drawing.Point[] cv = new Drawing.Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                OpenCvSharp.Point p = points[i];
                cv[i] = new Drawing.Point(p.X, p.Y);
            }

            return cv;
        }

        private bool Less(Drawing.Point a, Drawing.Point b, Drawing.Point center)
        {
            if (a.Y > 0)
            { //a between 0 and 180
                if (b.Y < 0)  //b between 180 and 360
                    return false;
                return a.X < b.X;
            }
            else
            { // a between 180 and 360
                if (b.Y > 0) //b between 0 and 180
                    return true;
                return a.X > b.X;
            }
        }

        static string RunTextRecog(Mat img)
        {
            OpenCvSharp.Text.OCRTesseract tesseract = OpenCvSharp.Text.OCRTesseract.Create(@"C:\Users\Flynn\AppData\Local\Tesseract-OCR\tessdata\", null, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
            string t;
            string[] textStrings;
            tesseract.Run(img, out t, out _, out textStrings, out _);

            StringBuilder sb = new StringBuilder();
            foreach (string s in textStrings)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }

        private double Angle(Drawing.Point start, Drawing.Point end)
        {
            return ((Math.PI / 2) - Math.Atan2(start.Y - end.Y, end.X - start.X)) * 180 / Math.PI;
        }

        private double Dist(Drawing.Point a, Drawing.Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private double Dist(OpenCvSharp.Point a, OpenCvSharp.Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        //private void Form1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    mouseClick = new Drawing.Point((int)(e.Location.X / imgScale), (int)(e.Location.Y / imgScale));
        //    int r = img.Width / 128;

        //    if (mouseClick.X >= img.Width || mouseClick.Y >= img.Height || mouseClick.X < 0 || mouseClick.Y < 0)
        //    {
        //        return;
        //    }
        //    Mat mouseArea;
        //    try
        //    {
        //        mouseArea = img.SubMat(Math.Max(mouseClick.Y - r, 0), Math.Min(mouseClick.Y + r, img.Width - 1), Math.Max(mouseClick.X - r, 0), Math.Min(mouseClick.X + r, img.Height - 1));
        //    }
        //    catch (Exception exception)
        //    {
        //        return;
        //    }

        //    //mouseArea = mouseArea.CvtColor(ColorConversionCodes.BGR2HSV);

        //    Scalar avg = Cv2.Sum(mouseArea);
        //    int area = mouseArea.Rows * mouseArea.Cols;
        //    avg.Val0 /= area;
        //    avg.Val1 /= area;
        //    avg.Val2 /= area;
        //    avg.Val3 = 0;

        //    //avg = CvtColor(avg, ColorConversionCodes.BGR2HSV);

        //    lowerB = new Scalar(avg.Val0 - hsvRadius.Val0, avg.Val1 - hsvRadius.Val1, avg.Val2 - hsvRadius.Val2);
        //    upperB = new Scalar(avg.Val0 + hsvRadius.Val0, avg.Val1 + hsvRadius.Val1, avg.Val2 + hsvRadius.Val2);

        //    //avg = CvtColor(avg, ColorConversionCodes.HSV2BGR);

        //    //lowerB = CvtColor(lowerB, ColorConversionCodes.HSV2BGR);
        //    //upperB = CvtColor(upperB, ColorConversionCodes.HSV2BGR);
        //    lowerB.Val3 = 0;
        //    upperB.Val3 = 255;
        //}

        private Scalar CvtColor(Scalar input, ColorConversionCodes cvt)
        {
            Mat outputMat = new Mat();
            Mat inputMat = new Mat(1, 1, MatType.CV_8UC3, input);

            Cv2.CvtColor(inputMat, outputMat, cvt);

            byte[] c = new byte[3];
            Marshal.Copy(outputMat.Data, c, 0, c.Length);
            //bgr.GetArray(out c);
            return new Scalar(c[0], c[1], c[2]);
        }
    }
}
