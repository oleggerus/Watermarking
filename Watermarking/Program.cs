using DAL.DAL;
using DAL.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using watermarking;

namespace Watermarking
{
    internal class Program
    {
        public static int Mode;
        public static int[] ValuesForBrightness = new[]
        {
            -200, -150, -100, -50, 0, 50, 100, 150, 200
        };
        public static int[] ValuesForContrast = new[]
        {
            -200, -150, -100, -50, 50, 100, 150, 200
        };

        public static async Task Main(string[] args)
        {

            Console.WriteLine("Choose your mode");
            Console.WriteLine("Press 1 to run one key for all containers");
            Console.WriteLine("Press 2 to run one container for all keys");

            var mode = Convert.ToInt32(Console.ReadLine());
            switch (mode)
            {
                case 1:
                    Mode = 1;
                    await RunOneKeyForAllContainers();
                    break;
                case 2:
                    Mode = 2;
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
                await RunDifferentBrightness(results, originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
                await RunDifferentContrast(results, originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
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

                await RunDifferentBrightness(results, originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
                await RunDifferentContrast(results, originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
            }
            await DalService.InsertResults(results);
        }


        private static async Task RunDifferentBrightness(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            Parallel.ForEach(ValuesForBrightness,
                async (da) =>
                {
                    var model = await ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, da, 0);
                    AddToResultSet(resultSet, model);
                });
        }

        private static async Task RunDifferentContrast(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
           Parallel.ForEach(ValuesForContrast,
                   async (da) =>
                   {
                       var model = await ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, 0, da);
                       AddToResultSet(resultSet, model);
                   });
        }

        private static async Task<WatermarkingResults> ProcessData(string originalFilePath, string originalKeyFileName, string fileNameToCreate, int brightness, int contrast)
        {
            fileNameToCreate = $"{fileNameToCreate}_{contrast}_{brightness}";
            var originalPathKey = Path.Combine(Constants.KeysFolderPath, originalKeyFileName);

            var originalKeyBitmap = new Bitmap(originalPathKey);

            var encryptionResult = await Encrypt(originalFilePath, fileNameToCreate, originalPathKey, brightness, contrast);
            var decryptionResult = await Decrypt(fileNameToCreate, originalKeyBitmap);

            var insertModel = Factory.PrepareResultModel(fileNameToCreate, originalKeyFileName, encryptionResult.Time,
                decryptionResult.Time, encryptionResult.Psnr, decryptionResult.Psnr, brightness, contrast);

            return insertModel;
        }


        private static async Task<ProcessingResult> Encrypt(string originalFilePath, string originalFileName, string originalKeyPath, int brightness, int contrast)
        {
            var originalContainerBitmap = new Bitmap(originalFilePath);
            var originalKeyBitmap = new Bitmap(originalKeyPath);

            var encryptionStopwatch = Stopwatch.StartNew();

            switch (Mode)
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

        private static void AddToResultSet(List<WatermarkingResults> results, WatermarkingResults model)
        {
            Console.WriteLine($"Processing case #{results.Count + 1}");
            results.Add(model);
        }
    }
}
