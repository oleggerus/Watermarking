using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Watermarking
{
    public static class Helpers
    {
        private static void SetContrast(Bitmap bmp, int threshold)
        {

            var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var oldColor = bmp.GetPixel(x, y);
                    var red = ((((oldColor.R / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                    var green = ((((oldColor.G / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                    var blue = ((((oldColor.B / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
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

        }

        public static Bitmap SetBrightness(Bitmap Image, int Value)
        {
            Bitmap TempBitmap = Image;
            float FinalValue = (float)Value / 255.0f;
            Bitmap NewBitmap = new System.Drawing.Bitmap(TempBitmap.Width, TempBitmap.Height);
            System.Drawing.Graphics NewGraphics = Graphics.FromImage(NewBitmap);
            float[][] FloatColorMatrix ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                };

            System.Drawing.Imaging.ColorMatrix NewColorMatrix = new System.Drawing.Imaging.ColorMatrix(FloatColorMatrix);
            System.Drawing.Imaging.ImageAttributes Attributes = new System.Drawing.Imaging.ImageAttributes();
            Attributes.SetColorMatrix(NewColorMatrix);
            NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, System.Drawing.GraphicsUnit.Pixel, Attributes);
            Attributes.Dispose();
            NewGraphics.Dispose();
            return NewBitmap;
        }

        public static double CalculatePSNR(Bitmap originalBitmap, Bitmap processedBitmap)
        {

            //cmode = radioButtonGrayscale.Checked;
            int col2 = originalBitmap.Width;
            //ComparePSNR(Container, Container_Reconstructed);
            //if (cmode == true)
            //    rows2 = originalBitmap.Height;
            //else
            int rows2 = originalBitmap.Height * 3;

            double MSE = 0;

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
            for (int i = 0; i < rows2 / 3; i++)
                for (int j = 0; j < col2; j++)
                {
                    byte processedPixelR = processedBitmap.GetPixel(i, j).R;
                    byte processedPixelG = processedBitmap.GetPixel(i, j).G;
                    byte processedPixelB = processedBitmap.GetPixel(i, j).B;

                    byte originalPixelR = originalBitmap.GetPixel(i, j).R;
                    byte originalPixelG = originalBitmap.GetPixel(i, j).G;
                    byte originalPixelB = originalBitmap.GetPixel(i, j).B;

                    MSE += Math.Pow((Math.Abs(processedPixelR - originalPixelR)), 2);
                    MSE += Math.Pow((Math.Abs(processedPixelG - originalPixelG)), 2);
                    MSE += Math.Pow((Math.Abs(processedPixelB - originalPixelB)), 2);
                }
            MSE = MSE / (rows2 * col2);
            double PSNR = 10 * Math.Log10(256 * 256 / MSE);
            //}
            return PSNR;
        }
    }
}
