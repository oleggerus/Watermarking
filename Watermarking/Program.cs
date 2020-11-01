using Algorithm;
using DAL.DAL;
using DAL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MainConstants = Constants.Constants;


namespace Watermarking
{
    internal class Program
    {
        public static int Mode;
        public static int[] ValuesForBrightness = {
            -200, -175, -150, -125, -100, -75, -50, -25, 0, 25, 50, 75, 100, 125, 150, 175, 200
        };
        public static int[] ValuesForContrast = {
            -200, -175, -150, -125, -100, -75, -50, -25, 25, 50, 75, 100, 125, 150, 175, 200
        };

        public static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Choose your mode");
                Console.WriteLine("Press 1 to run one key for all containers (with different contrasts and brightness)");
                Console.WriteLine("Press 2 to run one container for all keys (with different contrasts and brightness)");
                Console.WriteLine("Press 3 to run one container for one key");
                Console.WriteLine("Press 4 to run one key for all containers");

                Console.WriteLine();
                Console.WriteLine("Press 8 to see results");
                Console.WriteLine("Press 9 to see results (for different contrasts and brightness)");


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
                    case 3:
                        Mode = 3;
                        await RunOneContainerForOneKey();
                        break;
                    case 4:
                        Mode = 4;
                        await RunOneKeyForAllContainersNoContrastAndBrightness();
                        break;
                    case 8:
                        Mode = 8;
                        await GetResultsForSimpleMode();
                        break;
                    case 9:
                        Mode = 9;
                        await GetResultsForDiffContrastAndBrightness();
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Done");
            }
            
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

            foreach (var result in results)
            {
                result.Mode = 1;
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
            foreach (var result in results)
            {
                result.Mode = Mode;
            }
            await DalService.InsertResults(results);
        }

        private static async Task RunOneContainerForOneKey()
        {
            const string originalContainerFileName = "LenaOriginal512color";
            const string originalKeyFileName = "BabooOriginal128color";



            var originalFilePath = Path.Combine(MainConstants.ContainerFolderPath, $"{originalContainerFileName}.bmp");

            var model = await Executor.ProcessData(originalFilePath, $"{originalKeyFileName}.bmp", $"{originalContainerFileName}_{originalKeyFileName}", 0, 0, Mode);
            model.Mode = Mode;

            await DalService.InsertResult(model);
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

        private static async Task GetResultsForDiffContrastAndBrightness()
        {
            var items = (await DalService.GetAllResults()).Where(x=>x.Mode == 1 || x.Mode == 2).ToList();
            foreach (var item in items)
            {
                item.ContainerFileName = item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_'));
            }
          

            foreach (var container in items.GroupBy(x => x.ContainerFileName))
            {
                Console.WriteLine(); Console.WriteLine();

                Console.WriteLine($"Results for {container.Key}" );
                
                var brightnessResults = container.Where(x => x.Contrast == 0)
                    .OrderBy(x => x.Brightness);
                Console.WriteLine($"Results per brightness");
                Console.WriteLine("Brightness - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time");
                foreach (var data in brightnessResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18}",
                        data.Brightness, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds);
                }

                Console.WriteLine();
                var contrastResults = container.Where(x => x.Brightness == 0)
                    .OrderBy(x => x.Contrast);
                Console.WriteLine($"Results per contrast");
                Console.WriteLine("Contrast - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time");
                foreach (var data in contrastResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18}",
                        data.Contrast, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds);
                }
            }
        }
        private static async Task GetResultsForSimpleMode()
        {
            var items = (await DalService.GetAllResults()).Where(x=> x.Mode == 3 || x.Mode== 4).ToList();
            Console.WriteLine();
            Console.WriteLine("Red - Green - Blue - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time - Name");
            Console.WriteLine();
            foreach (var item in items)
            {
                item.ContainerFileName = item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_'));
                Console.WriteLine("{0,4} {1,5} {2,5} {3,18} {4,18} {5,16} {6,17} {7,20}",
                    item.AverageRedColor, item.AverageGreenColor, item.AverageBlueColor,
                    Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                    item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds,
                    item.ContainerFileName);
            }
        }

        private static async Task RunOneKeyForAllContainersNoContrastAndBrightness()
        {
            const string originalKeyFileName = "BabooOriginal128color.bmp";

            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();
            foreach (var originalFilePath in originalContainerPaths)
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, Path.GetFileNameWithoutExtension(originalFilePath), 0, 0, Mode);
                model.Mode = Mode;
                AddToResultSet(results, model);
            }

            await DalService.InsertResults(results);
        }
    }
}
