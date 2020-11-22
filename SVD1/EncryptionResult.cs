using System.Drawing;

namespace SVD
{
    public class EncryptionResult
    {
        public Bitmap InputContainer { get; set; }
        public Bitmap InputKey { get; set; }

        public Bitmap OutputContainer { get; set; }

        public int  AverageRedColor { get; set; }
        public int AverageGreenColor { get; set; }
        public int AverageBlueColor { get; set; }
        public int AverageRedColorWatermark { get; set; }
        public int AverageGreenColorWatermark { get; set; }
        public int AverageBlueColorWatermark { get; set; }

        public int ContainerHeight { get; set; }
        public int ContainerWidth { get; set; }

        public int WatermarkHeight { get; set; }
        public int WatermarkWidth{ get; set; }
        //public Bitmap OutputKey { get; set; }
    }
}
