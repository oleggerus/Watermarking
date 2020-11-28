using Algorithm;
using DAL;
using DAL.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MainConstants = Constants.Constants;


namespace Watermarking
{
    internal static class Program
    {
        private static int ParallelThreadsAmount = 20;

        public static int Mode;
        public static int[] ValuesForBrightness = {
            -200,  -150, -100, -50, 0, 50, 100, 150, 200
        };
        public static int[] ValuesForContrast = {
            -200,  -150, -100, -50, 0, 50, 100, 150, 200
        };
        public static int[] ValuesForNoise = {
            20, 40, 60, 80
        };

        public static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Choose your mode");
                Console.WriteLine("Press 1 to run all container with all keys");
                Console.WriteLine("Press 2 to run all containers with one key");
                Console.WriteLine("Press 3 to run all keys with one container");
                Console.WriteLine();
                Console.WriteLine("Press 4 to get all statistics");
                Console.WriteLine("Press 5 to get statistics per container");
                Console.WriteLine("Press 6 to get statistics per key");
                Console.WriteLine();

                var mode = Convert.ToInt32(Console.ReadLine());
                switch (mode)
                {
                    case 1:
                        Mode = (int)WatermarkingMode.AllToAll;
                        await RunAllNoContrastAndBrightness();
                        break;
                    case 2:
                        Console.WriteLine("Choose your options");
                        Console.WriteLine("Press 1 to run with original images");
                        Console.WriteLine("Press 2 to apply different contrast levels for key");
                        Console.WriteLine("Press 3 to apply different brightness levels for key");
                        Console.WriteLine("Press 4 to apply different noise levels for key");
                        Console.WriteLine();
                        mode = Convert.ToInt32(Console.ReadLine());
                        switch (mode)
                        {
                            case 1:
                                Mode = (int)WatermarkingMode.OneKeyToAllContainers;
                                await RunOneKeyForAllContainersNoContrastAndBrightness();
                                break;
                            case 2:
                                Mode = (int)WatermarkingMode.OneKeyToAllContainersWithContrast;
                                await RunOneKeyForAllContainers();
                                break;
                            case 3:
                                Mode = (int)WatermarkingMode.OneKeyToAllContainersWithBrightness;
                                await RunOneKeyForAllContainers();
                                break;
                            case 4:
                                Mode = (int)WatermarkingMode.OneKeyToAllContainersWithNoise;
                                await RunOneKeyForAllContainers();
                                break;
                        }
                        break;
                    case 3:
                        Console.WriteLine("Choose your options");
                        Console.WriteLine("Press 1 to run with original images");
                        Console.WriteLine("Press 2 to apply different contrast levels for container");
                        Console.WriteLine("Press 3 to apply different brightness levels for container");
                        Console.WriteLine("Press 4 to apply different noise levels for container");
                        Console.WriteLine();
                        mode = Convert.ToInt32(Console.ReadLine());
                        switch (mode)
                        {
                            case 1:
                                Mode = (int)WatermarkingMode.OneContainerToAllKeys;
                                await RunOneContainerForAllKeysNoContrastAndBrightness();
                                break;
                            case 2:
                                Mode = (int)WatermarkingMode.OneContainerToAllKeysWithBrightness;
                                await RunOneContainerForAllKeys();
                                break;
                            case 3:
                                Mode = (int)WatermarkingMode.OneContainerToAllKeysWithContrast;
                                await RunOneContainerForAllKeys();
                                break;
                            case 4:
                                Mode = (int)WatermarkingMode.OneContainerToAllKeysWithNoise;
                                await RunOneContainerForAllKeys();
                                break;
                        }
                        break;

                    case 4:
                        await GetResultsForSimpleMode();
                        break;
                    case 5:
                        await GetResultsForContainerByLevel();
                        break;
                    case 6:
                        await GetResultsForKeyByLevel();
                        break;

                }
            }

        }

        private static async Task RunAllNoContrastAndBrightness()
        {
            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);
            var originalKeysPaths = Directory.GetFiles(MainConstants.KeysDiffFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);


            var i = 1;

            foreach (var originalKeyPath in originalKeysPaths)
            {
                foreach (var originalFilePath in originalContainerPaths)
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(originalFilePath)}_{Path.GetFileNameWithoutExtension(originalKeyPath)}";
                    var model = await Executor.ProcessData(originalFilePath, Path.GetFileName(originalKeyPath),
                                          newFileName,
                                          0, 0, 0, Mode);
                    model.Mode = Mode;
                    Console.WriteLine($"Processing case #{i++} for {newFileName}");
                    await DalService.InsertResult(model);
                }
            }
        }

        private static async Task RunOneKeyForAllContainers()
        {
            const string originalKeyFileName = "Baboo128color.bmp";

            var originalContainerPaths = Directory.GetFiles(MainConstants.ContainerFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            var results = new List<WatermarkingResults>();

            await ForEachAsync(originalContainerPaths, 20, async originalFilePath =>
            {
                if (Mode == (int)WatermarkingMode.OneKeyToAllContainersWithBrightness)
                {
                    await RunDifferentBrightness(results, originalFilePath, originalKeyFileName,
                    Path.GetFileNameWithoutExtension(originalFilePath));
                }
                if (Mode == (int)WatermarkingMode.OneKeyToAllContainersWithContrast)
                {
                    await RunDifferentContrast(results, originalFilePath, originalKeyFileName,
                        Path.GetFileNameWithoutExtension(originalFilePath));
                }
                if (Mode == (int)WatermarkingMode.OneKeyToAllContainersWithNoise)
                {
                    await RunDifferentNoise(results, originalFilePath, originalKeyFileName,
                        Path.GetFileNameWithoutExtension(originalFilePath));
                }
            });

            foreach (var result in results.ToList())
            {
                result.Mode = Mode;
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
                if (Mode == (int)WatermarkingMode.OneContainerToAllKeysWithBrightness)
                {
                    await RunDifferentBrightness(results, originalFilePath, originalKeyName,
                    Path.GetFileNameWithoutExtension(originalKeyPath));
                }
                if (Mode == (int)WatermarkingMode.OneContainerToAllKeysWithContrast)
                {
                    await RunDifferentContrast(results, originalFilePath, originalKeyName,
                        Path.GetFileNameWithoutExtension(originalKeyPath));
                }
                if (Mode == (int)WatermarkingMode.OneContainerToAllKeysWithNoise)
                {
                    await RunDifferentNoise(results, originalFilePath, originalKeyName,
                        Path.GetFileNameWithoutExtension(originalKeyPath));
                }

            });

            foreach (var result in results.ToList())
            {
                result.Mode = Mode;
            }
            await DalService.InsertResults(results);
        }

        private static async Task RunDifferentBrightness(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            await ForEachAsync(ValuesForBrightness, ParallelThreadsAmount, async brightness =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, brightness, 0, 0, Mode);
                AddToResultSet(resultSet, model);
            });
        }

        private static async Task RunDifferentContrast(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            await ForEachAsync(ValuesForContrast, ParallelThreadsAmount, async contrast =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, 0, contrast, 0, Mode);
                AddToResultSet(resultSet, model);
            });
        }

        private static async Task RunDifferentNoise(List<WatermarkingResults> resultSet, string originalFilePath, string originalKeyFileName, string fileNameToCreate)
        {
            await ForEachAsync(ValuesForNoise, ParallelThreadsAmount, async noise =>
            {
                var model = await Executor.ProcessData(originalFilePath, originalKeyFileName, fileNameToCreate, 0, 0, noise, Mode);
                AddToResultSet(resultSet, model);
            });
        }

        private static void AddToResultSet(List<WatermarkingResults> results, WatermarkingResults model)
        {
            Console.WriteLine($"Processing case #{results.Count + 1}  - {model.ContainerFileName}");
            results.Add(model);
        }

        private static async Task GetResultsForContainerByLevel()
        {
            var items = (await DalService.GetAllResults()).Where(x =>
                x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithBrightness ||
                x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithContrast ||
                x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithNoise).ToList();
            foreach (var item in items)
            {
                item.ContainerFileName = item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_'));
            }


            foreach (var container in items.GroupBy(x => x.ContainerFileName))
            {
                Console.WriteLine(); Console.WriteLine();

                Console.WriteLine($"Results for {container.Key}");

                var brightnessResults = container.Where(x => x.Contrast == 0 && x.Noise == 0)
                    .OrderBy(x => x.Brightness);
                Console.WriteLine($"Results per brightness");
                Console.WriteLine("Brightness - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time - Key");
                foreach (var data in brightnessResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Brightness, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds, data.KeyFileName);
                }

                Console.WriteLine();
                var contrastResults = container.Where(x => x.Brightness == 0 && x.Noise == 0)
                    .OrderBy(x => x.Contrast);
                Console.WriteLine($"Results per contrast");
                Console.WriteLine("Contrast - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time - Key");
                foreach (var data in contrastResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Contrast, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds, data.KeyFileName);
                }

                Console.WriteLine();
                var noiseResults = container.Where(x => x.Brightness == 0 && x.Contrast == 0)
                    .OrderBy(x => x.Noise);
                Console.WriteLine($"Results per noise");
                Console.WriteLine("Noise - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time  -  Key");
                foreach (var data in noiseResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Noise, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds, data.KeyFileName);
                }

                Console.WriteLine("_______________________________________________________________________________________________________");
                Console.WriteLine();

            }
        }

        private static async Task GetResultsForKeyByLevel()
        {
            var items = (await DalService.GetAllResults()).Where(x =>
                x.Mode == (int)WatermarkingMode.OneKeyToAllContainersWithBrightness ||
                x.Mode == (int)WatermarkingMode.OneKeyToAllContainersWithContrast ||
                x.Mode == (int)WatermarkingMode.OneKeyToAllContainersWithNoise).ToList();

            foreach (var container in items.GroupBy(x => x.KeyFileName))
            {
                Console.WriteLine(); Console.WriteLine();

                Console.WriteLine($"Results for {container.Key}");

                var brightnessResults = container.Where(x => x.Contrast == 0 && x.Noise == 0)
                    .OrderBy(x => x.Brightness);
                Console.WriteLine($"Results per brightness");
                Console.WriteLine("Brightness - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time - Container");
                foreach (var data in brightnessResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Brightness, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds,
                        data.ContainerFileName.Substring(0, data.ContainerFileName.IndexOf('_')));
                }

                Console.WriteLine();
                var contrastResults = container.Where(x => x.Brightness == 0 && x.Noise == 0)
                    .OrderBy(x => x.Contrast);
                Console.WriteLine($"Results per contrast");
                Console.WriteLine("Contrast - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time - Container");
                foreach (var data in contrastResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Contrast, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds,
                        data.ContainerFileName.Substring(0, data.ContainerFileName.IndexOf('_')));
                }

                Console.WriteLine();
                var noiseResults = container.Where(x => x.Brightness == 0 && x.Contrast == 0)
                    .OrderBy(x => x.Noise);
                Console.WriteLine($"Results per noise");
                Console.WriteLine("Noise - Encryption PSNR - Decryption PSNR - Encryption time - Decryption time  -  Container");
                foreach (var data in noiseResults)
                {
                    Console.WriteLine("{0,8} {1,15} {2,15} {3,18} {4,18} {5,18}",
                        data.Noise, Math.Round(data.EncryptionPsnr, 2), Math.Round(data.DecryptionPsnr, 2),
                        data.EncryptionTime.TotalMilliseconds, data.DecryptionTime.TotalMilliseconds,
                        data.ContainerFileName.Substring(0, data.ContainerFileName.IndexOf('_')));
                }

                Console.WriteLine(
                    "_______________________________________________________________________________________________________");
                Console.WriteLine();
            }
        }


        private static async Task GetResultsForSimpleMode()
        {
            var items = (await DalService.GetAllResults()).Where(x =>
                x.Mode == (int)WatermarkingMode.AllToAll || x.Mode == (int)WatermarkingMode.OneKeyToAllContainers ||
                x.Mode == (int)WatermarkingMode.OneContainerToAllKeys).ToList();
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
                    Path.GetFileNameWithoutExtension(originalFilePath), 0, 0, 0, Mode);
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
                    Path.GetFileNameWithoutExtension(originalKeyPath), 0, 0, 0, Mode);
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
                select Task.Run(async delegate
                {
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
