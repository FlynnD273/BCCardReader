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
    using System.Runtime.InteropServices;

    using ImageFormat = System.Drawing.Imaging.ImageFormat;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

    public sealed partial class Bitmap
    {
        #region METHODS

        public override void Save(Stream stream, ImageFormat format)
        {
            Android.Graphics.Bitmap.CompressFormat compressFormat;
            if (format.Equals(ImageFormat.Jpeg))
            {
                compressFormat = Android.Graphics.Bitmap.CompressFormat.Jpeg;
            }
            else if (format.Equals(ImageFormat.Png))
            {
                compressFormat = Android.Graphics.Bitmap.CompressFormat.Png;
            }
            else
            {
                throw new ArgumentOutOfRangeException("format", format, "Only supported formats are JPEG and PNG");
            }

            var androidBitmap = (Android.Graphics.Bitmap)this;
            androidBitmap.Compress(compressFormat, 100, stream);
        }

        public override void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            throw new NotImplementedException("PCL");
        }

        internal static Bitmap Create(Stream stream)
        {
            var androidBitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
            return androidBitmap == null ? null : (Bitmap)androidBitmap;
        }

        private static PixelFormat GetPixelFormat(Android.Graphics.Format androidPixelFormat)
        {
            switch (androidPixelFormat)
            {
               case Android.Graphics.Format.A8:
                    return PixelFormat.PAlpha;
                case Android.Graphics.Format.Rgb565:
                    return PixelFormat.Format16bppRgb565;
                case Android.Graphics.Format.Rgb888:
                    return PixelFormat.Format24bppRgb;
                case Android.Graphics.Format.Rgba5551:
                    return PixelFormat.Format16bppArgb1555;
                case Android.Graphics.Format.Rgba8888:
                    return PixelFormat.Format32bppPArgb;
                default:
                    throw new ArgumentOutOfRangeException(
                        "androidPixelFormat",
                        androidPixelFormat,
                        "Unsupported Android pixel format.");
            }
        }

        private static Android.Graphics.Bitmap.Config GetAndroidBitmapConfig(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppPArgb:
                    return Android.Graphics.Bitmap.Config.Argb8888;
                case PixelFormat.Format16bppRgb565:
                    return Android.Graphics.Bitmap.Config.Rgb565;
                case PixelFormat.PAlpha:
                    return Android.Graphics.Bitmap.Config.Alpha8;
                default:
                    throw new ArgumentOutOfRangeException(
                        "pixelFormat",
                        pixelFormat,
                        "Unsupported pixel format for Android");
            }
        }

        private static Bitmap GetAndroidAdaptedBitmap(Bitmap bitmap)
        {
            switch (bitmap._pixelFormat)
            {
                case PixelFormat.PAlpha:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format32bppPArgb:
                    return bitmap;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Indexed:
                    return bitmap.Clone(PixelFormat.Format32bppPArgb);
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                    return bitmap.Clone(PixelFormat.Format16bppRgb565);
                case PixelFormat.Alpha:
                    return bitmap.Clone(PixelFormat.PAlpha);
                default:
                    throw new ArgumentOutOfRangeException(
                        "pixelFormat",
                        bitmap._pixelFormat,
                        "Unsupported pixel format for Android");
            }
        }

        #endregion

        #region OPERATORS

        public static explicit operator Android.Graphics.Bitmap(Bitmap bitmap)
        {
            var adaptedBitmap = GetAndroidAdaptedBitmap(bitmap);

            var width = adaptedBitmap._width;
            var height = adaptedBitmap._height;
            var stride = adaptedBitmap._stride / 4;

            var pixels = new int[height * stride];
            Marshal.Copy(adaptedBitmap._scan0, pixels, 0, pixels.Length);

            var config = GetAndroidBitmapConfig(adaptedBitmap._pixelFormat);

            var androidBitmap = Android.Graphics.Bitmap.CreateBitmap(width, height, config);
            androidBitmap.SetPixels(pixels, 0, stride, 0, 0, width, height);

#if EVALUATION
            var textSize = (float)(width / 25);
            var c = new Android.Graphics.Canvas(androidBitmap);
            var p = new Android.Graphics.Paint();
            p.SetARGB(255, 255, 0, 0);
            p.TextSize = textSize;
            c.DrawText("For evaluation only.", 1f, height - 1f - 3f * textSize, p);
            c.DrawText("Contact licenses@cureos.com", 1f, height - 1f - 2f * textSize, p);
            c.DrawText("for full version.", 1f, height - 1f - textSize, p);
#endif
            return androidBitmap;
        }

        public static explicit operator Bitmap(Android.Graphics.Bitmap androidBitmap)
        {
            var width = androidBitmap.Width;
            var height = androidBitmap.Height;
            var rowBytes = androidBitmap.RowBytes;

            var pixelFormat = GetPixelFormat(androidBitmap.GetBitmapInfo().Format);

            var pixels = new int[height * rowBytes / 4];
            androidBitmap.GetPixels(pixels, 0, rowBytes / 4, 0, 0, width, height);

            unsafe
            {
                fixed (int* ptr = pixels)
                {
                    return new Bitmap(width, height, rowBytes, pixelFormat, (IntPtr)ptr);
                }
            }
        }

        #endregion
    }
}
