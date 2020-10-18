using DAL.DAL;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using watermarking;
using Watermarking;
using MainConstants = Constants.Constants;

namespace Algorithm
{
    public static class Executor
    {
        public static async Task<WatermarkingResults> ProcessData(string originalFilePath, string originalKeyFileName, string fileNameToCreate, int brightness, int contrast, int mode)
        {
            fileNameToCreate = $"{fileNameToCreate}_{contrast}_{brightness}";
            var originalPathKey = Path.Combine(MainConstants.KeysFolderPath, originalKeyFileName);

            var originalKeyBitmap = new Bitmap(originalPathKey);

            var encryptionResult = await Encrypt(originalFilePath, fileNameToCreate, originalPathKey, brightness, contrast, mode);
            var decryptionResult = await Decrypt(fileNameToCreate, originalKeyBitmap);

            var insertModel = Factory.PrepareResultModel(fileNameToCreate, originalKeyFileName, encryptionResult.Time,
                decryptionResult.Time, encryptionResult.Psnr, decryptionResult.Psnr, brightness, contrast);

            return insertModel;
        }


        public static async Task<ProcessingResult> Encrypt(string originalFilePath, string originalFileName, string originalKeyPath, int brightness, int contrast, int mode)
        {
            var originalContainerBitmap = new Bitmap(originalFilePath);
            var originalKeyBitmap = new Bitmap(originalKeyPath);

            var encryptionStopwatch = Stopwatch.StartNew();

            switch (mode)
            {
                case 1:
                    originalContainerBitmap =
                        Helpers.SetContrast(Helpers.SetBrightness(originalContainerBitmap, brightness), contrast);
                    break;
                case 2:
                    originalKeyBitmap =
                        Helpers.SetContrast(Helpers.SetBrightness(originalKeyBitmap, brightness), contrast);
                    break;
            }

            var result = await Svd.Encrypt(originalContainerBitmap, originalKeyBitmap, originalFileName);


            encryptionStopwatch.Stop();
            var encryptionPsnr = Helpers.CalculatePsnr(result.InputContainer, result.OutputContainer);

            return new ProcessingResult
            {
                Psnr = encryptionPsnr,
                Time = encryptionStopwatch.Elapsed,
            };
        }
        public static async Task<ProcessingResult> Decrypt(string originalFileName, Bitmap originalKeyBitmap)
        {
            var decryptionFileName = $"{originalFileName}_Container";
            var decryptionFilePath = Path.Combine(MainConstants.ContainersProcessedPath, $"{decryptionFileName}.bmp");

            var decryptionStopwatch = Stopwatch.StartNew();
            var decryptionResult = await Svd.Decrypt(new Bitmap(decryptionFilePath), decryptionFileName);
            decryptionStopwatch.Stop();
            var decryptionPsnr = Helpers.CalculatePsnr(originalKeyBitmap, decryptionResult);

            return new ProcessingResult
            {
                Psnr = decryptionPsnr,
                Time = decryptionStopwatch.Elapsed
            };
        }
    }
}
