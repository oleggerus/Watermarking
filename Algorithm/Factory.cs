using System;
using DAL.DAL;

namespace Algorithm
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
            int r,
            int g,
            int b
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
                AverageRedColor = r,
                AverageGreenColor = g,
                AverageBlueColor = b
            };
        }
    }
}
