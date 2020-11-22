using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DAL;
using SVD;
using MainConstants = Constants.Constants;

namespace Algorithm
{
    public static class Executor
    {
        public static async Task<WatermarkingResults> ProcessData(string originalFilePath, string originalKeyFileName, string fileNameToCreate, int brightness, int contrast, int mode)
        {
            fileNameToCreate = $"{fileNameToCreate}_{contrast}_{brightness}_{mode}";

            var originalPathKey =
                Path.Combine(mode == 5 || mode == 0 ? MainConstants.KeysDiffFolderPath : MainConstants.KeysFolderPath,
                    originalKeyFileName);

            var originalKeyBitmap = new Bitmap(originalPathKey);

            var encryptionResult = await Encrypt(originalFilePath, fileNameToCreate, originalPathKey, brightness, contrast, mode);
            var decryptionResult = await Decrypt(fileNameToCreate, originalKeyBitmap);

            var insertModel = Factory.PrepareResultModel(fileNameToCreate, originalKeyFileName, encryptionResult.Time,
                decryptionResult.Time, encryptionResult.Psnr, decryptionResult.Psnr, brightness, contrast,
                encryptionResult.AverageRedColor, encryptionResult.AverageGreenColor, encryptionResult.AverageBlueColor,
                encryptionResult.AverageRedColorWatermark, encryptionResult.AverageGreenColorWatermark, encryptionResult.AverageBlueColorWatermark,
                encryptionResult.ContainerWidth, encryptionResult.ContainerHeight,
                encryptionResult.WatermarkWidth, encryptionResult.WatermarkHeight);

            originalKeyBitmap.Dispose();
            return insertModel;
        }


        public static async Task<ProcessingResult> Encrypt(string originalFilePath, string originalFileName, string originalKeyPath, int brightness, int contrast, int mode)
        {
            var originalContainerBitmap = new Bitmap(originalFilePath);
            var originalKeyBitmap = new Bitmap(originalKeyPath);


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

            return await HandleEncryption(originalContainerBitmap, originalKeyBitmap, originalFileName);
        }
        public static async Task<ProcessingResult> HandleEncryption(Bitmap originalContainerBitmap, Bitmap originalKeyBitmap, string originalFileName)
        {
            var encryptionStopwatch = Stopwatch.StartNew();

            var result = await Svd.Encrypt(originalContainerBitmap, originalKeyBitmap, originalFileName);

            encryptionStopwatch.Stop();
            var encryptionPsnr = Helpers.CalculatePsnr(result.InputContainer, result.OutputContainer);

            return new ProcessingResult
            {
                Psnr = encryptionPsnr,
                Time = encryptionStopwatch.Elapsed,
                AverageBlueColor = result.AverageBlueColor,
                AverageGreenColor = result.AverageGreenColor,
                AverageRedColor = result.AverageRedColor,
                AverageBlueColorWatermark = result.AverageBlueColorWatermark,
                AverageGreenColorWatermark = result.AverageGreenColorWatermark,
                AverageRedColorWatermark = result.AverageRedColorWatermark,
                ContainerHeight = result.ContainerHeight,
                ContainerWidth = result.ContainerWidth,
                WatermarkHeight = result.WatermarkHeight,
                WatermarkWidth = result.WatermarkWidth,
                ContainerWithWatermark = result.OutputContainer
            };
        }

        public static async Task<ProcessingResult> Decrypt(string originalFileName, Bitmap originalKeyBitmap)
        {
            var decryptionFileName = $"{originalFileName}_Container";
            var decryptionFilePath = Path.Combine(MainConstants.ContainersProcessedPath, $"{decryptionFileName}.bmp");

            var decryptionStopwatch = Stopwatch.StartNew();
            var decryptionResult = await Svd.Decrypt(new Bitmap(decryptionFilePath), decryptionFileName,
                originalKeyBitmap.Width, originalKeyBitmap.Height);
            decryptionStopwatch.Stop();
            var decryptionPsnr = Helpers.CalculatePsnr(originalKeyBitmap, decryptionResult);
            originalKeyBitmap.Dispose();
            return new ProcessingResult
            {
                Psnr = decryptionPsnr,
                Time = decryptionStopwatch.Elapsed,
                ExtractedWatermark = decryptionResult
            };
        }

        public static async Task<ProcessingResult> DecryptFromBitmap(Bitmap encrypptedContainer, string originalFileName, Bitmap originalKeyBitmap)
        {
            var decryptionFileName = $"{originalFileName}_Container";            

            var decryptionStopwatch = Stopwatch.StartNew();
            var decryptionResult = await Svd.Decrypt(encrypptedContainer, decryptionFileName,
                originalKeyBitmap.Width, originalKeyBitmap.Height);
            decryptionStopwatch.Stop();
            var decryptionPsnr = Helpers.CalculatePsnr(originalKeyBitmap, decryptionResult);
            originalKeyBitmap.Dispose();
            return new ProcessingResult
            {
                Psnr = decryptionPsnr,
                Time = decryptionStopwatch.Elapsed,
                ExtractedWatermark = decryptionResult
            };
        }

    }
}
