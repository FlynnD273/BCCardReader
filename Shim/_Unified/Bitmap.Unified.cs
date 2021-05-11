/*
 *  Copyright (c) 2013-2016, Cureos AB.
 *  All rights reserved.
 *  http://www.cureos.com
 *
 *	This file is part of Shim.NET.
 *
 *  Shim.NET is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Shim.NET is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Shim.NET.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace System.Drawing
{
    using System.Drawing.Imaging;
    using System.IO;

    using CoreGraphics;

    using Foundation;

    using UIKit;

    public sealed partial class Bitmap
    {
        #region METHODS

        public override void Save(Stream stream, ImageFormat format)
        {
            var uiImage = new UIImage((CGImage)this);

            NSData compressedImage;
            if (format.Equals(ImageFormat.Jpeg))
            {
                compressedImage = uiImage.AsJPEG();
            }
            else if (format.Equals(ImageFormat.Png))
            {
                compressedImage = uiImage.AsPNG();
            }
            else
            {
                throw new ArgumentOutOfRangeException("format", format, "Only supported formats are JPEG and PNG");
            }

            compressedImage.AsStream().CopyTo(stream);
        }

        public override void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            throw new NotImplementedException("PCL");
        }

        internal static Bitmap Create(Stream stream)
        {
            return (Bitmap)UIImage.LoadFromData(NSData.FromStream(stream)).CGImage;
        }

        private static Bitmap GetTouchAdaptedBitmap(Bitmap bitmap)
        {
            switch (bitmap._pixelFormat) {
            case PixelFormat.Format16bppGrayScale:
            case PixelFormat.Format16bppRgb555:
            case PixelFormat.Format16bppRgb565:
            case PixelFormat.Format24bppRgb:
            case PixelFormat.Format32bppRgb:
            case PixelFormat.Format48bppRgb:
                return bitmap.Clone (PixelFormat.Format32bppRgb);
            case PixelFormat.Format16bppArgb1555:
            case PixelFormat.Format32bppArgb:
            case PixelFormat.Format32bppPArgb:
            case PixelFormat.Format64bppArgb:
            case PixelFormat.Format64bppPArgb:
            case PixelFormat.Format32bppRgbaProprietary:
            case PixelFormat.Format32bppPRgbaProprietary:
                return bitmap.Clone (PixelFormat.Format32bppPArgb);
            case PixelFormat.Format1bppIndexed:
                return bitmap.Clone (PixelFormat.Format32bppPArgb); // TODO Change to Format1bppIndexed
            case PixelFormat.Format4bppIndexed:
                return bitmap.Clone (PixelFormat.Format32bppPArgb);// TODO Change to Format4bppIndexed
            case PixelFormat.Format8bppIndexed:
            case PixelFormat.Indexed:
                return bitmap.Clone (PixelFormat.Format32bppPArgb);// TODO Change to Format8bppIndexed
            case PixelFormat.Alpha:
            case PixelFormat.PAlpha:
                return bitmap.Clone (PixelFormat.PAlpha);
            default:
                throw new ArgumentOutOfRangeException (
                    "pixelFormat",
                    bitmap._pixelFormat,
                    "Unsupported pixel format for iOS");
            }
        }

        private static CGColorSpace CreateColorSpace (ColorPalette palette)
        {
            throw new NotImplementedException ();
        }

        /// <summary>
        /// Gets a Bitmap pixel format from the <see cref="CGImage"/> properties.
        /// </summary>
        /// <returns>The pixel format.</returns>
        /// <param name="cgImage">CoreGraphics image.</param>
        /// <remarks>List of iOS and Mac OSX supported pixel formats can be found 
        /// <see cref="https://developer.apple.com/library/mac/documentation/graphicsimaging/conceptual/drawingwithquartz2d/dq_context/dq_context.html#//apple_ref/doc/uid/TP30001066-CH203-CJBEAGHH">here</see>.
        /// </remarks>
        private static PixelFormat GetPixelFormat(CGImage cgImage)
        {
            if (cgImage == null)
                throw new ArgumentNullException ("cgImage");

            var alphaInfo = cgImage.AlphaInfo;
            if (cgImage.ColorSpace == null && alphaInfo == CGImageAlphaInfo.Only)
                return PixelFormat.Alpha;

            var bitsPerComponent = cgImage.BitsPerComponent;
            var bitsPerPixel = cgImage.BitsPerPixel;
            var model = cgImage.ColorSpace.Model;
            var unsupportedText =
                String.Format(
                    "Unsupported CGImage format. Model: {0}, Alpha: {1}, Bits per pixel: {2}, Bits per component: {3}",
                    model,
                    alphaInfo,
                    bitsPerPixel,
                    bitsPerComponent);

            switch (model) {
            case CGColorSpaceModel.RGB:
                switch (bitsPerPixel) {
                case 16:
                    switch (alphaInfo) {
                    case CGImageAlphaInfo.NoneSkipFirst:
                        return PixelFormat.Format16bppRgb555;
                    default:
                        throw new ArgumentException (unsupportedText);
                    }
                case 32:
                    switch (alphaInfo) {
                    case CGImageAlphaInfo.None:
                    case CGImageAlphaInfo.NoneSkipLast:
                        return PixelFormat.Format32bppRgb;
                    case CGImageAlphaInfo.Last:
                        return PixelFormat.Format32bppArgb;
                    case CGImageAlphaInfo.PremultipliedLast:
                        return PixelFormat.Format32bppPArgb;
                    case CGImageAlphaInfo.First:
                    case CGImageAlphaInfo.NoneSkipFirst:
                        return PixelFormat.Format32bppRgbaProprietary;
                    case CGImageAlphaInfo.PremultipliedFirst:
                        return PixelFormat.Format32bppPRgbaProprietary;
                    default:
                        throw new ArgumentException (unsupportedText);
                    }
                case 64:
                    switch (alphaInfo) {
                    case CGImageAlphaInfo.PremultipliedLast:
                        return PixelFormat.Format64bppPArgb;
                    case CGImageAlphaInfo.NoneSkipLast:
                        throw new NotImplementedException ("Support for 64 bpp xRGB images not yet implemented");
                    default:
                        throw new ArgumentException (unsupportedText);
                    }
                default:
                    throw new ArgumentException (unsupportedText);
                }
            case CGColorSpaceModel.Monochrome:
                switch (bitsPerComponent) {
                case 8:
                    throw new NotImplementedException ("Support for 8 bpp gray images not yet implemented");
                case 16:
                    return PixelFormat.Format16bppGrayScale;
                default:
                    throw new ArgumentException (unsupportedText);
                }
            case CGColorSpaceModel.Indexed:
                var baseComponents = cgImage.ColorSpace.GetBaseColorSpace ().Components;
                var indexCount = cgImage.ColorSpace.GetColorTable ().Length / baseComponents;

                if (indexCount < 2)
                    return PixelFormat.Format1bppIndexed;
                if (indexCount < 16)
                    return PixelFormat.Format4bppIndexed;
                if (indexCount < 256)
                    return PixelFormat.Format8bppIndexed;

                throw new ArgumentException (unsupportedText);
            default:
                throw new ArgumentException (unsupportedText);
            }
        }

        private static ColorPalette CreatePalette(CGColorSpace colorSpace)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region OPERATORS

        public static explicit operator CGImage(Bitmap bitmap)
        {
            var adaptedBitmap = GetTouchAdaptedBitmap (bitmap);

            int bitsPerComponent;
            CGColorSpace colorSpace;
            CGImageAlphaInfo alphaInfo;

            switch (adaptedBitmap._pixelFormat) {
            case PixelFormat.Format32bppRgb:
                bitsPerComponent = 8;
                colorSpace = CGColorSpace.CreateDeviceRGB ();
                alphaInfo = CGImageAlphaInfo.NoneSkipLast;
                break;
            case PixelFormat.Format32bppPArgb:
                bitsPerComponent = 8;
                colorSpace = CGColorSpace.CreateDeviceRGB ();
                alphaInfo = CGImageAlphaInfo.PremultipliedLast;
                break;
            case PixelFormat.Format64bppPArgb:
                bitsPerComponent = 16;
                colorSpace = CGColorSpace.CreateDeviceRGB ();
                alphaInfo = CGImageAlphaInfo.PremultipliedLast;
                break;
            case PixelFormat.Format1bppIndexed:
            case PixelFormat.Format4bppIndexed:
            case PixelFormat.Format8bppIndexed:
                // TODO CGBitmapContext does not support indexed color spaces; consider changing to CGImage constructor
                throw new NotImplementedException ("Support for indexed pixel formats not implemented");
            case PixelFormat.PAlpha:
                throw new NotImplementedException ("Support for alpha-only pixel formats not implemented");
            default:
                throw new InvalidOperationException ();
            }

            using (
                var context = new CGBitmapContext(
                    adaptedBitmap._scan0,
                    adaptedBitmap._width,
                    adaptedBitmap._height,
                    bitsPerComponent,
                    adaptedBitmap._stride,
                    colorSpace,
                    alphaInfo))
            {
#if EVALUATION
                var fontSize = (nfloat)(adaptedBitmap._width / 25);
                context.SelectFont("Helvetica", fontSize, CGTextEncoding.MacRoman);
                context.SetFillColor(new CGColor(255f, 0f, 0f));
                context.SetTextDrawingMode(CGTextDrawingMode.Fill);
                context.ShowTextAtPoint(1f, 1f + 2f * fontSize, "For evaluation only.", 20);
                context.ShowTextAtPoint(1f, 1f + fontSize, "Contact licenses@cureos.com", 27);
                context.ShowTextAtPoint(1f, 1f, "for full version.", 17);
#endif
                return context.ToImage();
            }
        }

        public static explicit operator Bitmap(CGImage cgImage)
        {
            var width = (int)cgImage.Width;
            var pixelFormat = GetPixelFormat(cgImage);

            var bitmap = new Bitmap(
                width,
                (int)cgImage.Height,
                GetStride(width, pixelFormat),
                pixelFormat,
                cgImage.DataProvider.CopyData().Bytes);

            if (cgImage.ColorSpace.Model == CGColorSpaceModel.Indexed) bitmap.Palette = CreatePalette(cgImage.ColorSpace);

            return bitmap;
        }

        #endregion
    }
}
