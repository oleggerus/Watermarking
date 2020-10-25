using System.Drawing;

namespace Watermarking
{
    public class EncryptionResult
    {
        public Bitmap InputContainer { get; set; }
        public Bitmap InputKey { get; set; }

        public Bitmap OutputContainer { get; set; }

        public int  AverageRedColor { get; set; }
        public int AverageGreenColor { get; set; }
        public int AverageBlueColor { get; set; }
        //public Bitmap OutputKey { get; set; }
    }
}
