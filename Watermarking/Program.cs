using System;
using System.Drawing;
using System.IO;
using watermarking;

namespace Watermarking
{
    class Program
    {
        const string ContainerFolderPath = @"..\..\..\..\Containers\";
        const string KeysFolderPath = @"..\..\..\..\Keys\";

        static void Main(string[] args)
        {
            var pathContainer = Path.Combine(ContainerFolderPath, "LenaOriginal512color.bmp");
            var containerBitmap = new Bitmap(pathContainer);

            var pathKey = Path.Combine(KeysFolderPath, "BabooOriginal128color.bmp");
            var keyBitmap = new Bitmap(pathKey);

            var result = SVD.Encrypt(containerBitmap, keyBitmap);
            Console.ReadLine();
        }
    }
}
