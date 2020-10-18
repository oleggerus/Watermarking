using Algorithm;
using DAL.DAL;
using DAL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MainConstants = Constants.Constants;


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

            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerFolderPath, "*.bmp",
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

            var originalFilePath = Path.Combine(MainConstants.ContainerFolderPath, "LenaOriginal512color.bmp");
            var originalKeysPaths = Directory.GetFiles(MainConstants.KeysFolderPath, "*.bmp",
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
                    var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, da, 0, Mode);
                    AddToResultSet(resultSet, model);
                });
        }

        private static async Task RunDifferentContrast(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
           Parallel.ForEach(ValuesForContrast,
                   async (da) =>
                   {
                       var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, 0, da, Mode);
                       AddToResultSet(resultSet, model);
                   });
        }

        

        private static void AddToResultSet(List<WatermarkingResults> results, WatermarkingResults model)
        {
            Console.WriteLine($"Processing case #{results.Count + 1}");
            results.Add(model);
        }
    }
}
