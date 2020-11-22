using Algorithm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using DAL.Services;
using MainConstants = Constants.Constants;


namespace Watermarking
{
    internal static class Program
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
                Console.WriteLine("Press 0 to run all containers with all keys");
                Console.WriteLine();
                Console.WriteLine("Press 1 to run one key for all containers (with different contrasts and brightness)");
                Console.WriteLine("Press 2 to run one container for all keys (with different contrasts and brightness)");
                Console.WriteLine();
                Console.WriteLine("Press 3 to run one container for one key");
                Console.WriteLine("Press 4 to run one key for all containers");
                Console.WriteLine("Press 5 to run one container for all keys");


                Console.WriteLine();
                Console.WriteLine("Press 8 to see results");
                Console.WriteLine("Press 9 to see results (for different contrasts and brightness)");

                Console.WriteLine();
                Console.WriteLine("Your choice:");
                var mode = Convert.ToInt32(Console.ReadLine());
                switch (mode)
                {
                    case 0:
                        Mode = 0;
                        RunAllNoContrastAndBrightness();
                        break;
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
                    case 5:
                        Mode = 5;
                        await RunOneContainerForAllKeysNoContrastAndBrightness();
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
                Console.WriteLine();
                Console.WriteLine();
            }
            
        }


        private static async Task RunAllNoContrastAndBrightness()
        {
            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);
            var originalKeysPaths = Directory.GetFiles(MainConstants.KeysDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);


            var i = 1;
            //Parallel.ForEach(originalKeysPaths, originalKeyPath =>
            //{
            //    Parallel.ForEach(originalContainerPaths, async originalFilePath =>
            //     {
            //         var newFileName = $"{Path.GetFileNameWithoutExtension(originalFilePath)}_{Path.GetFileNameWithoutExtension(originalKeyPath)}";
            //         var model = await Executor.ProcessData(originalFilePath, Path.GetFileName(originalKeyPath),
            //                               newFileName,
            //                               0, 0, Mode);
            //         model.Mode = Mode;
            //         Console.WriteLine($"Processing case #{i++} for {newFileName}");
            //         await DalService.InsertResult(model);
            //     });
            //});

            foreach (var originalKeyPath in originalKeysPaths)
            {
                foreach (var originalFilePath in originalContainerPaths)
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(originalFilePath)}_{Path.GetFileNameWithoutExtension(originalKeyPath)}";
                    var model = await Executor.ProcessData(originalFilePath, Path.GetFileName(originalKeyPath),
                                          newFileName,
                                          0, 0, Mode);
                    model.Mode = Mode;
                    Console.WriteLine($"Processing case #{i++} for {newFileName}");
                    await DalService.InsertResult(model);
                }
            }
                //}
                //Parallel.ForEach(originalKeysPaths, originalKeyPath =>
                //{
                //    Parallel.ForEach(originalContainerPaths, async originalFilePath =>
                //    {

                //    });
                //});

                //await ForEachAsync(originalKeysPaths, 20, async originalKeyPath =>
                //{
                //    await ForEachAsync(originalContainerPaths, 30, async originalFilePath =>
                //    {
                //        var model = await Executor.ProcessData(originalFilePath, Path.GetFileName(originalKeyPath),
                //            $"{Path.GetFileNameWithoutExtension(originalFilePath)}_{Path.GetFileNameWithoutExtension(originalKeyPath)}",
                //            0, 0, Mode);
                //        model.Mode = Mode;
                //        AddToResultSet(results, model);
                //    });
                //});
            }

        private static async Task RunOneKeyForAllContainers()
        {
            const string originalKeyFileName = "Baboo128color.bmp";

            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();

            await ForEachAsync(originalContainerPaths, 20, async originalFilePath =>
            {
                await RunDifferentBrightness(results, originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
                await RunDifferentContrast(results, originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
            });

            foreach (var result in results.ToList())
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
            await ForEachAsync(originalKeysPaths, 20, async originalKeyPath =>
            {
                var originalKeyName = Path.GetFileName(originalKeyPath);

                await RunDifferentBrightness(results, originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
                await RunDifferentContrast(results, originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
            });

            foreach (var result in results.ToList())
            {
                result.Mode = Mode;
            }
            await DalService.InsertResults(results);
        }

        private static async Task RunOneContainerForOneKey()
        {
            const string originalContainerFileName = "LenaOriginal512color";
            const string originalKeyFileName = "Baboo128color";

            var originalFilePath = Path.Combine(MainConstants.ContainerFolderPath, $"{originalContainerFileName}.bmp");

            var model = await Executor.ProcessData(originalFilePath, $"{originalKeyFileName}.bmp",
                $"{originalContainerFileName}_{originalKeyFileName}", 0, 0, Mode);
            model.Mode = Mode;

            await DalService.InsertResult(model);
        }

        private static async Task RunDifferentBrightness(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            await ForEachAsync(ValuesForBrightness, 20, async brightness =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, brightness, 0, Mode);
                AddToResultSet(resultSet, model);
            });
        }

        private static async Task RunDifferentContrast(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            await ForEachAsync(ValuesForContrast, 20, async contrast =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, 0, contrast, Mode);
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
            const string originalKeyFileName = "Baboo128color.bmp";

            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();

            await ForEachAsync(originalContainerPaths, 20, async originalFilePath =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath), 0, 0, Mode);
                model.Mode = Mode;
                AddToResultSet(results, model);
            });

            await DalService.InsertResults(results);
        }

        private static async Task RunOneContainerForAllKeysNoContrastAndBrightness()
        {
            const string originalContainerFileName = "LenaOriginal512color";
            var originalFilePath = Path.Combine(MainConstants.ContainerFolderPath, $"{originalContainerFileName}.bmp");

            var originalKeysPaths = Directory.GetFiles(MainConstants.KeysDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();

            await ForEachAsync(originalKeysPaths, 20, async originalKeyPath =>
            {
                var model = await Executor.ProcessData(originalFilePath, Path.GetFileName(originalKeyPath),
                    Path.GetFileNameWithoutExtension(originalKeyPath), 0, 0, Mode);
                model.Mode = Mode;
                AddToResultSet(results, model);
            });

            await DalService.InsertResults(results);
        }

        public static Task ForEachAsync<T>(
            this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current).ContinueWith(t =>
                            {
                                //observe exceptions
                            });

                }));
        }

    }
}
