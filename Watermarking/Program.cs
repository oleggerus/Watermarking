using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
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

            var pathKey = Path.Combine(Constants.KeysFolderPath, "BabooOriginal128color.bmp");
            var keyOriginalBitmap = new Bitmap(pathKey);

            var containerPaths = Directory.GetFiles(Constants.ContainerFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            foreach (var filePath in containerPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var result = await Svd.Encrypt(new Bitmap(filePath), keyOriginalBitmap, fileName);
                var psnr = Helpers.CalculatePsnr(result.InputContainer, result.OutputContainer);
            }


            var containerDecryptedPaths = Directory.GetFiles(Constants.ContainersProcessedPath, "*.bmp",
                SearchOption.TopDirectoryOnly);
            foreach (var filePath in containerDecryptedPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var result = await Svd.Decrypt(new Bitmap(filePath), fileName);
                var psnr = Helpers.CalculatePsnr(keyOriginalBitmap, result);

            }

        }

        private static async Task RunOneContainerForAllKeys()
        {

            var pathContainer = Path.Combine(Constants.ContainerFolderPath, "LenaOriginal512color.bmp");
            var keysPaths = Directory.GetFiles(Constants.KeysFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            foreach (var keyPath in keysPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(keyPath);
                var result = await Svd.Encrypt(new Bitmap(pathContainer), new Bitmap(keyPath), fileName);
            }


            var containerDecryptedPaths = Directory.GetFiles(Constants.ContainersProcessedPath, "*.bmp",
                SearchOption.TopDirectoryOnly);
            foreach (var filePath in containerDecryptedPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var result = await Svd.Decrypt(new Bitmap(filePath), fileName);
            }

        }
    }
}
