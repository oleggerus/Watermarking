using System;
using System.Drawing;

namespace Algorithm
{
    public class ProcessingResult
    {
        public TimeSpan Time { get; set; }
        public double Psnr { get; set; }
        public double Mse { get; set; }
        public int AverageRedColor { get; set; }
        public int AverageGreenColor { get; set; }
        public int AverageBlueColor { get; set; }
        public int AverageRedColorWatermark { get; set; }
        public int AverageGreenColorWatermark { get; set; }
        public int AverageBlueColorWatermark { get; set; }
        public int ContainerHeight { get; set; }
        public int ContainerWidth { get; set; }

        public int WatermarkHeight { get; set; }
        public int WatermarkWidth { get; set; }

        public Bitmap ContainerWithWatermark { get; set; }
        public Bitmap ExtractedWatermark { get; set; }
    }
}
