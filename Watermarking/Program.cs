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
            var pathContainer = Path.Combine(Constants.ContainerFolderPath, "LenaOriginal512color.bmp");

            var pathKey = Path.Combine(Constants.KeysFolderPath, "BabooOriginal128color.bmp");


            var containerPaths = Directory.GetFiles(Constants.ContainerFolderPath, "*.bmp",
                SearchOption.TopDirectoryOnly);

            foreach (var filePath in containerPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var result = await Svd.Encrypt(new Bitmap(filePath), new Bitmap(pathKey), fileName);
            }


            var containerDecryptedPaths = Directory.GetFiles(Constants.ContainersProcessedPath, "*.bmp",
                SearchOption.TopDirectoryOnly);
            foreach (var filePath in containerDecryptedPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var result = await Svd.Decrypt(new Bitmap(filePath), fileName);
            }


            Console.ReadLine();
        }
    }
}
