using System;
using System.Collections.Generic;
using System.Text;
using DAL.DAL;

namespace Watermarking
{
    internal static class Factory
    {
        public static WatermarkingResults PrepareResultModel(
            string containerFileName,
            string keyFileName,
            TimeSpan encryptionTime,
            TimeSpan decryptionTime,
            double encryptionPsnr,
            double decryptionPsnr
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
            };
        }
    }
}
