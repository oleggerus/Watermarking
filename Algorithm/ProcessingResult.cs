using System;

namespace Algorithm
{
    public class ProcessingResult
    {
        public TimeSpan Time { get; set; }
        public double Psnr { get; set; }
        public int AverageRedColor { get; set; }
        public int AverageGreenColor { get; set; }
        public int AverageBlueColor { get; set; }
    }
}
