using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SVD
{
    public static class Helpers
    {
        public static Bitmap SetNoise(Bitmap image, int percent)
        {    
            var filter = new SaltAndPepperNoise(percent);    
            filter.ApplyInPlace(image);

            return image;
        }

        public static Bitmap SetContrast(Bitmap bmp, int threshold)
        {

            var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);

            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    var oldColor = bmp.GetPixel(x, y);
                    var red = ((oldColor.R / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                    var green = ((oldColor.G / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                    var blue = ((oldColor.B / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                    if (red > 255) red = 255;
                    if (red < 0) red = 0;
                    if (green > 255) green = 255;
                    if (green < 0) green = 0;
                    if (blue > 255) blue = 255;
                    if (blue < 0) blue = 0;

                    var newColor = Color.FromArgb(oldColor.A, (int)red, (int)green, (int)blue);
                    bmp.SetPixel(x, y, newColor);
                }
            }

            return bmp;

        }

        /// <summary>
        /// Set brightness for the image
        /// </summary>
        /// <param name="image">input bitmap</param>
        /// <param name="value">value from -255 to 255</param>
        /// <returns></returns>
        public static Bitmap SetBrightness(Bitmap image, int value)
        {
            var tempBitmap = image;
            var finalValue = value / 255.0f;
            var newBitmap = new Bitmap(tempBitmap.Width, tempBitmap.Height);
            var newGraphics = Graphics.FromImage(newBitmap);
            float[][] floatColorMatrix ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new[] {finalValue, finalValue, finalValue, 1, 1}
                };

            var newColorMatrix = new System.Drawing.Imaging.ColorMatrix(floatColorMatrix);
            var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(newColorMatrix);
            newGraphics.DrawImage(tempBitmap, new System.Drawing.Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height), 0, 0, tempBitmap.Width, tempBitmap.Height, GraphicsUnit.Pixel, attributes);
            attributes.Dispose();
            newGraphics.Dispose();
            return newBitmap;
        }

        public static double CalculatePsnr(Bitmap originalBitmap, Bitmap processedBitmap)
        {

            //cmode = radioButtonGrayscale.Checked;
            var col2 = originalBitmap.Width;
            //ComparePSNR(Container, Container_Reconstructed);
            //if (cmode == true)
            //    rows2 = originalBitmap.Height;
            //else
            var rows2 = originalBitmap.Height * 3;

            double mse = 0;

            //grey mode
            //if (cmode == true)
            //{
            //    for (int i = 0; i < rows2; i++)
            //        for (int j = 0; j < col2; j++)
            //        {
            //            processedPixel = processedBitmap.GetPixel(i, j).ToArgb() & 0x000000ff;
            //            originalPixel = originalBitmap.GetPixel(i, j).ToArgb() & 0x000000ff;
            //            MSE += Math.Pow((Math.Abs(processedPixel - originalPixel)), 2);
            //        }
            //    MSE = MSE / (rows2 * col2);
            //    PSNR = 10 * Math.Log10(256 * 256 / MSE);
            //}
            //else
            //{
            for (var i = 0; i < rows2 / 3; i++)
                for (var j = 0; j < col2; j++)
                {
                    var processedPixelR = processedBitmap.GetPixel(i, j).R;
                    var processedPixelG = processedBitmap.GetPixel(i, j).G;
                    var processedPixelB = processedBitmap.GetPixel(i, j).B;

                    var originalPixelR = originalBitmap.GetPixel(i, j).R;
                    var originalPixelG = originalBitmap.GetPixel(i, j).G;
                    var originalPixelB = originalBitmap.GetPixel(i, j).B;

                    mse += Math.Pow(Math.Abs(processedPixelR - originalPixelR), 2);
                    mse += Math.Pow(Math.Abs(processedPixelG - originalPixelG), 2);
                    mse += Math.Pow(Math.Abs(processedPixelB - originalPixelB), 2);
                }
            mse /= rows2 * col2;
            var psnr = 10 * Math.Log10(256 * 256 / mse);
            //}
            return psnr;
        }

        public static Tuple<int, int, int> CalculateColors(Bitmap inputContainer)
        {
            var srcData = inputContainer.LockBits(
                new Rectangle(0, 0, inputContainer.Width, inputContainer.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            var totals = new long[] { 0, 0, 0 };

            var width = inputContainer.Width;
            var height = inputContainer.Height;

            unsafe
            {
                var p = (byte*)(void*)Scan0;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        for (var color = 0; color < 3; color++)
                        {
                            var idx = (y * stride) + x * 4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            var avgB = (int)totals[0] / (width * height);
            var avgG = (int)totals[1] / (width * height);
            var avgR = (int)totals[2] / (width * height);

            inputContainer.UnlockBits(srcData);
            return new Tuple<int, int, int>(avgR, avgG, avgB);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }
    }
}
