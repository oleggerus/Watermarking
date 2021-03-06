﻿using DAL;
using DAL.Services;
using SVD;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using MainConstants = Constants.Constants;

namespace Algorithm
{
    public static class Executor
    {
        public static async Task<WatermarkingResults> ProcessData(string originalFilePath, string originalKeyFileName, string fileNameToCreate, int brightness, int contrast, int noise, int mode)
        {
            fileNameToCreate = $"{fileNameToCreate}_{contrast}_{brightness}_{noise}_{mode}";

            var originalPathKey =
                Path.Combine(mode == 5 || mode == (int)WatermarkingMode.AllToAll ? MainConstants.KeysDiffFolderPath : MainConstants.KeysFolderPath,
                    originalKeyFileName);

            var originalKeyBitmap = new Bitmap(originalPathKey);

            var encryptionResult = await Encrypt(originalFilePath, fileNameToCreate, originalPathKey, brightness, contrast, noise, mode);
            var decryptionResult = await Decrypt(fileNameToCreate, originalKeyBitmap);

            var insertModel = Factory.PrepareResultModel(fileNameToCreate, originalKeyFileName, encryptionResult.Time,
                decryptionResult.Time, encryptionResult.Psnr, decryptionResult.Psnr, 
                encryptionResult.Mse, decryptionResult.Mse,
                brightness, contrast, noise,
                encryptionResult.AverageRedColor, encryptionResult.AverageGreenColor, encryptionResult.AverageBlueColor,
                encryptionResult.AverageRedColorWatermark, encryptionResult.AverageGreenColorWatermark, encryptionResult.AverageBlueColorWatermark,
                encryptionResult.ContainerWidth, encryptionResult.ContainerHeight,
                encryptionResult.WatermarkWidth, encryptionResult.WatermarkHeight);

            originalKeyBitmap.Dispose();
            return insertModel;
        }


        public static async Task<ProcessingResult> Encrypt(string originalFilePath, string originalFileName, string originalKeyPath, int brightness, int contrast, int noise, int mode)
        {
            var originalContainerBitmap = new Bitmap(originalFilePath);
            var originalKeyBitmap = new Bitmap(originalKeyPath);


            switch (mode)
            {
                case (int)WatermarkingMode.OneKeyToAllContainersWithNoise:
                    originalKeyBitmap = Helpers.SetNoise(originalKeyBitmap, noise);
                    break;
                case (int)WatermarkingMode.OneKeyToAllContainersWithBrightness:
                    originalKeyBitmap = Helpers.SetBrightness(originalKeyBitmap, brightness);
                    break;
                case (int)WatermarkingMode.OneKeyToAllContainersWithContrast:
                    originalKeyBitmap =Helpers.SetContrast(originalKeyBitmap, contrast);
                    break;
                case (int)WatermarkingMode.OneContainerToAllKeysWithNoise:
                    originalContainerBitmap = Helpers.SetNoise(originalContainerBitmap, noise);
                    break;
                case (int)WatermarkingMode.OneContainerToAllKeysWithBrightness:
                    originalContainerBitmap = Helpers.SetBrightness(originalContainerBitmap, brightness);
                    break;
                case (int)WatermarkingMode.OneContainerToAllKeysWithContrast:
                    originalContainerBitmap = Helpers.SetContrast(originalContainerBitmap, contrast);
                    break;

            }

            return await HandleEncryption(originalContainerBitmap, originalKeyBitmap, originalFileName);
        }
        public static async Task<ProcessingResult> HandleEncryption(Bitmap originalContainerBitmap, Bitmap originalKeyBitmap, string originalFileName)
        {
            var encryptionStopwatch = Stopwatch.StartNew();

            var result = await Svd.Encrypt(originalContainerBitmap, originalKeyBitmap, originalFileName);

            encryptionStopwatch.Stop();

            var encryptionMse = Helpers.CalculateMse(result.InputContainer, result.OutputContainer);
            var encryptionPsnr = Helpers.CalculatePsnr(encryptionMse);

            return new ProcessingResult
            {
                Psnr = encryptionPsnr,
                Mse = encryptionMse,
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
            var decryptionMse = Helpers.CalculateMse(originalKeyBitmap, decryptionResult);
            var decryptionPsnr = Helpers.CalculatePsnr(decryptionMse);
            
            originalKeyBitmap.Dispose();
            return new ProcessingResult
            {
                Psnr = decryptionPsnr,
                Mse = decryptionMse,
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
            var decryptionMse = Helpers.CalculateMse(originalKeyBitmap, decryptionResult);
            var decryptionPsnr = Helpers.CalculatePsnr(decryptionMse);
            originalKeyBitmap.Dispose();
            return new ProcessingResult
            {
                Psnr = decryptionPsnr,
                Mse = decryptionMse,
                Time = decryptionStopwatch.Elapsed,
                ExtractedWatermark = decryptionResult
            };
        }

    }
}
