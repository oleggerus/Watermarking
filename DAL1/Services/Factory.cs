using System;
namespace DAL.Services
{
    public static class Factory
    {
        public static WatermarkingResults PrepareResultModel(
            string containerFileName,
            string keyFileName,
            TimeSpan encryptionTime,
            TimeSpan decryptionTime,
            double encryptionPsnr,
            double decryptionPsnr,
            int brightness,
            int contrast,
            int noise,
            int r,
            int g,
            int b,
            int rW,
            int gW,
            int bW,
            int cWidth,
            int cHeight,
            int wWidth,
            int wHeight
        )
        {
            return new WatermarkingResults
            {
                ContainerFileName = containerFileName,
                KeyFileName = keyFileName,
                DecryptionPsnr = decryptionPsnr,
                EncryptionPsnr = encryptionPsnr,
                EncryptionTime = encryptionTime,
                DecryptionTime = decryptionTime,
                CreatedOn = DateTime.Now,
                Brightness = brightness,
                Contrast = contrast,
                Noise = noise,
                AverageRedColor = r,
                AverageGreenColor = g,
                AverageBlueColor = b,
                AverageRedColorWatermark = rW,
                AverageGreenColorWatermark = gW,
                AverageBlueColorWatermark = bW,
                ContainerHeight = cHeight,
                ContainerWidth = cWidth,
                WatermarkWidth = wWidth,
                WatermarkHeight = wHeight
            };
        }
    }
}
