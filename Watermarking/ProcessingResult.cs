using System;
using System.Collections.Generic;
using System.Text;

namespace Watermarking
{
    public class ProcessingResult
    {
        public TimeSpan Time { get; set; }
        public double Psnr { get; set; }
        public int Contrast{ get; set; }
        public int Brightness { get; set; }
    }
}
