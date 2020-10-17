using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAL.DAL;
using DAL.Services;
using watermarking;

namespace Watermarking
{
    internal class Program
    {

        public static async Task Main(string[] args)
        {

            Console.WriteLine("Choose your mode");
            Console.WriteLine("Press 1 to run one key for all containers");
            Console.WriteLine("Press 2 to run one container for all keys");
            
            var mode = Convert.ToInt32(Console.ReadLine());
            switch (mode)
            {
                case 1:
                    await RunOneKeyForAllContainers();
                    break;
                case 2:
                    await RunOneContainerForAllKeys();
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task RunOneKeyForAllContainers()
        {
            const string originalKeyFileName = "BabooOriginal128color.bmp";
            
            var originalContainerPaths = Directory.GetFiles(Constants.ContainerFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();
            foreach (var originalFilePath in originalContainerPaths)
            {
                var model = await ProcessData(originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
                results.Add(model);
            }

            await DalService.InsertResults(results);
        }

        private static async Task RunOneContainerForAllKeys()
        {

            var originalFilePath = Path.Combine(Constants.ContainerFolderPath, "LenaOriginal512color.bmp");
            var originalKeysPaths = Directory.GetFiles(Constants.KeysFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();
            foreach (var originalKeyPath in originalKeysPaths)
            {
                var originalKeyName = Path.GetFileName(originalKeyPath);

                var model = await ProcessData(originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
                results.Add(model);
            }
            await DalService.InsertResults(results);
        }


        private static async Task<WatermarkingResults> ProcessData(string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            var originalPathKey = Path.Combine(Constants.KeysFolderPath, originalKeyFileName);

            var originalKeyBitmap = new Bitmap(originalPathKey);

            var encryptionResult = await Encrypt(originalFilePath, fileNameToCreate, originalKeyBitmap);
            var decryptionResult = await Decrypt(fileNameToCreate, originalKeyBitmap);

            var insertModel = Factory.PrepareResultModel(fileNameToCreate, originalKeyFileName, encryptionResult.Time,
                decryptionResult.Time, encryptionResult.Psnr, decryptionResult.Psnr);

            return insertModel;
        }

        private static async Task<ProcessingResult> Encrypt(string originalFilePath, string originalFileName, Bitmap originalKeyBitmap)
        {
            var encryptionStopwatch = Stopwatch.StartNew();
            var result = await Svd.Encrypt(new Bitmap(originalFilePath), originalKeyBitmap, originalFileName);
            encryptionStopwatch.Stop();
            var encryptionPsnr = Helpers.CalculatePsnr(result.InputContainer, result.OutputContainer);

            return new ProcessingResult
            {
                Psnr = encryptionPsnr,
                Time = encryptionStopwatch.Elapsed
            };
        }
        private static async Task<ProcessingResult> Decrypt(string originalFileName, Bitmap originalKeyBitmap)
        {
            var decryptionFileName = $"{originalFileName}_Container";
            var decryptionFilePath = Path.Combine(Constants.ContainersProcessedPath, $"{decryptionFileName}.bmp");

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
