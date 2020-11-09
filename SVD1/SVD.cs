using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MainConstants = Constants.Constants;

namespace SVD
{

    public static class Svd
    {


        public static async Task<EncryptionResult> Encrypt(Bitmap container, Bitmap watermark, string fileName)
        {
            var frameDimension = container.Width / watermark.Width;
            var principalComponentsNumber = frameDimension * frameDimension;


            // зчитування пікселів лени та бабуїна
            var stringWithOriginalContainerPixels = new StringBuilder();

            for (var x = 0; x < container.Width; x++)
            {
                for (var y = 0; y < container.Height; y++)
                {
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(container.GetPixel(x, y).R));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(container.GetPixel(x, y).G));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(container.GetPixel(x, y).B));
                }
            }

            var stringWithOriginalWatermarkPixels = new StringBuilder();
            for (var i = 0; i < watermark.Height; i++)
                for (var j = 0; j < watermark.Width; j++)
                {
                    stringWithOriginalWatermarkPixels.Append(Convert.ToChar(watermark.GetPixel(i, j).R));
                    stringWithOriginalWatermarkPixels.Append(Convert.ToChar(watermark.GetPixel(i, j).G));
                    stringWithOriginalWatermarkPixels.Append(Convert.ToChar(watermark.GetPixel(i, j).B));
                }

            var matrixMain = new double[container.Height * 3, container.Width];

            int tmpR = 0, tmpG = 1, tmpB = 2;
            for (var i = 0; i < container.Height * 3; i += 3)
                for (var j = 0; j < container.Width; j++)
                {
                    matrixMain[i, j] = stringWithOriginalContainerPixels[tmpR];
                    matrixMain[i + 1, j] = stringWithOriginalContainerPixels[tmpG];
                    matrixMain[i + 2, j] = stringWithOriginalContainerPixels[tmpB];
                    tmpR += 3;
                    tmpG += 3;
                    tmpB += 3;
                }

            // конвертація основної матриці 1536х512 в principalComponentsNumber-ти стовпцеву
            var columns = frameDimension * frameDimension;
            var rows = container.Width / frameDimension * (container.Height / frameDimension) * 3;
            var matrixMainConverted = new double[rows, columns];

            var kMax = container.Height;
            var numberOfSquares = kMax / frameDimension;

            for (var i = 0; i < rows; i++)
                for (var j = 0; j < columns; j++)
                {
                    //Matrix_Main[i, j] = (double)((byte)container.GetPixel(((i * frame) / (int)(Math.Sqrt(rows * col))) * frame + j / frame, (i % ((int)(Math.Sqrt(rows * col)) / frame)) * frame + j % frame).ToArgb() & 0x000000ff);
                    var k1 = j / frameDimension + i / numberOfSquares * frameDimension;
                    var l1 = j % frameDimension + i % numberOfSquares * frameDimension;
                    matrixMainConverted[i, j] = matrixMain[k1, l1];
                }

            // центрування конвертованої матриці
            var columnMean = matrixMainConverted.Mean(0);
            var matrixMainConvertedCentered = matrixMainConverted.Subtract(columnMean, 0);

            //SVD
            var svd = new SingularValueDecomposition(matrixMainConvertedCentered);
            var eigenVectors = svd.RightSingularVectors;
            var singularValues = svd.Diagonal;
            var _eigenVectors = eigenVectors;
            var _singularValues = singularValues;

            var eigenvalues = singularValues.Pow(2);
            eigenvalues.Divide(matrixMainConverted.GetLength(0) - 1);

            // пошук середнього для пікселів бабуїна
            var tmpSum = 0;
            for (var i = 0; i < stringWithOriginalWatermarkPixels.Length; i++)
                tmpSum += (int)stringWithOriginalWatermarkPixels[i];
            var tmpAvg = tmpSum / stringWithOriginalWatermarkPixels.Length;

            // заміна principalComponentsNumber-ої компоненти на центровані значення(пікселів) бабуїна
            var matrixMainComponents = matrixMainConvertedCentered.DotWithTransposed(eigenVectors.Transpose());
            for (var i = 0; i < matrixMainComponents.GetLength(0); i++)
            {
                matrixMainComponents[i, principalComponentsNumber - 1] = stringWithOriginalWatermarkPixels[i] - tmpAvg;
            }

            // реконструкція конвертованої матриці
            var backToMatrixMainConvertedCentered = matrixMainComponents.DotWithTransposed(eigenVectors);
            var matrixReconstructed = backToMatrixMainConvertedCentered.Add(columnMean, 0);


            // повна реконструкція матриці
            var matrixMainReconstructedFully = new double[container.Height * 3, container.Width];
            for (var i = 0; i < matrixReconstructed.GetLength(0); i++)
            {
                for (var j = 0; j < matrixReconstructed.GetLength(1); j++)
                {
                    var k2 = j / frameDimension + i / numberOfSquares * frameDimension;
                    var l2 = j % frameDimension + i % numberOfSquares * frameDimension;
                    matrixMainReconstructedFully[k2, l2] = matrixReconstructed[i, j];
                }
            }

            // вивід кольорового зображення після певних перетворень
            int tmpR1 = 0, tmpG1 = 1, tmpB1 = 2;
            for (var i = 0; i < container.Width * 3; i += 3)
                for (var j = 0; j < container.Height; j++)
                {
                    stringWithOriginalContainerPixels[tmpR1] = (char)matrixMainReconstructedFully[i, j];
                    stringWithOriginalContainerPixels[tmpG1] = (char)matrixMainReconstructedFully[i + 1, j];
                    stringWithOriginalContainerPixels[tmpB1] = (char)matrixMainReconstructedFully[i + 2, j];
                    tmpR1 += 3;
                    tmpG1 += 3;
                    tmpB1 += 3;
                }

            var p = 0;
            var containerReconstructed = new Bitmap(container.Height, container.Width);
            for (var i = 0; i < container.Width; i++)
                for (var j = 0; j < container.Height; j++)
                {
                    var processedPixelR = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelR < 0) processedPixelR = 0;
                    if (processedPixelR > 255) processedPixelR = 255;
                    p++;
                    var processedPixelG = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelG < 0) processedPixelG = 255;
                    if (processedPixelG > 255) processedPixelG = 0;
                    p++;
                    var processedPixelB = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelB < 0) processedPixelB = 0;
                    if (processedPixelB > 255) processedPixelB = 255;
                    p++;
                    containerReconstructed.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixelR), Convert.ToByte(processedPixelG), Convert.ToByte(processedPixelB)));
                }


            await SaveAllDataToFiles(containerReconstructed, fileName, _eigenVectors, _singularValues);

            return PrepareEncryptionResult(container,  watermark, containerReconstructed);
        }

        private static async Task SaveAllDataToFiles(Image containerProcessed, string fileName, double[,] outputKey1, IEnumerable<double> outputKey2)
        {
            var newFileName = $"{fileName}_Container";
            containerProcessed.Save(Path.Combine(MainConstants.ContainersProcessedPath, $"{newFileName}.bmp"));

            using (var key1Writer = new StreamWriter(Path.Combine(MainConstants.DecryptKeysPath, $"{newFileName}_EigenVectors.txt")))
            {
                for (var i = 0; i < outputKey1.GetLength(0); i++)
                {
                    for (var j = 0; j < outputKey1.GetLength(1); j++)
                    {
                        await key1Writer.WriteAsync($"{outputKey1[i, j]} ");
                    }
                    await key1Writer.WriteLineAsync();
                }
            }


            using var key2Writer = new StreamWriter(Path.Combine(MainConstants.DecryptKeysPath, $"{newFileName}_SingularValues.txt"));
            foreach (var key in outputKey2)
            {
                await key2Writer.WriteAsync($"{key} ");
            }
        }

        /// <summary>
        /// Returns decrypted watermark
        /// </summary>
        /// <param name="encryptedContainer">container with watermark</param>
        /// <param name="fileName">container file name</param>
        /// <returns></returns>
        public static async Task<Bitmap> Decrypt(Bitmap encryptedContainer, string fileName, int watermarkWidth, int watermarkHeight)
        {
            var frameDimension = encryptedContainer.Width / watermarkWidth;
            var principalComponentsNumber = frameDimension * frameDimension;
            var watermarkPixelsColoredArraySize = watermarkWidth * watermarkHeight * 3;




            var _eigenVectors = Path.Combine(MainConstants.DecryptKeysPath, $"{fileName}_EigenVectors.txt");
            var _singularValues = Path.Combine(MainConstants.DecryptKeysPath, $"{fileName}_SingularValues.txt");



            // зчитування пікселів лени та бабуїна
            var stringWithOriginalContainerPixels = new StringBuilder();
            for (var x = 0; x < encryptedContainer.Width; x++)
                for (var y = 0; y < encryptedContainer.Height; y++)
                {
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).R));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).G));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).B));
                }

            var arrayWithWatermarkPixelsColorful = new double[watermarkPixelsColoredArraySize];

            var matrixMain = new double[encryptedContainer.Height * 3, encryptedContainer.Width];

            int tmpR = 0, tmpG = 1, tmpB = 2;
            for (var i = 0; i < encryptedContainer.Width * 3; i += 3)
                for (var j = 0; j < encryptedContainer.Height; j++)
                {
                    matrixMain[i, j] = stringWithOriginalContainerPixels[tmpR];
                    matrixMain[i + 1, j] = stringWithOriginalContainerPixels[tmpG];
                    matrixMain[i + 2, j] = stringWithOriginalContainerPixels[tmpB];
                    tmpR += 3;
                    tmpG += 3;
                    tmpB += 3;
                }

            // конвертація основної матриці 1536х512 16-ти стовпцеву

            var columns = frameDimension * frameDimension;

            var rows = encryptedContainer.Width / frameDimension * (encryptedContainer.Height / frameDimension) * 3;
            var matrixMainConverted = new double[rows, columns];

            var kMax = encryptedContainer.Height;
            var numberOfSquares = kMax / frameDimension;


            for (var i = 0; i < rows; i++)
                for (var j = 0; j < columns; j++)
                {
                    //Matrix_Main[i, j] = (double)((byte)encryptedContainer.GetPixel(((i * frame) / (int)(Math.Sqrt(rows * col))) * frame + j / frame, (i % ((int)(Math.Sqrt(rows * col)) / frame)) * frame + j % frame).ToArgb() & 0x000000ff);

                    var k1 = j / frameDimension + i / numberOfSquares * frameDimension;
                    var l1 = j % frameDimension + i % numberOfSquares * frameDimension;
                    matrixMainConverted[i, j] = matrixMain[k1, l1];
                }

            // центрування конвертованої матриці
            var columnMean = matrixMainConverted.Mean(0);
            var matrixMainConvertedCentered = matrixMainConverted.Subtract(columnMean, 0);

            // дешифрування
            // зчитування ключів

            var eigenVectors = new double[principalComponentsNumber, principalComponentsNumber];
            int numberOfLines = 0, numberOfColumns = 0;
            string line;
            using (var reader = new StreamReader(_eigenVectors))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    numberOfColumns = line.Split(' ').Length;
                    numberOfLines++;
                }
            }

            var array2D = new string[numberOfLines, numberOfColumns];
            numberOfLines = 0;

            using (var reader = new StreamReader(_eigenVectors))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var tempArray = line.Split(' ');
                    for (var i = 0; i < tempArray.Length; ++i)
                    {
                        array2D[numberOfLines, i] = tempArray[i];
                    }
                    numberOfLines++;
                }
            }

            for (var i = 0; i < principalComponentsNumber; i++)
                for (var j = 0; j < principalComponentsNumber; j++)
                    eigenVectors[i, j] = Convert.ToDouble(array2D[i, j]);


            var singularValues1 = new double[1, principalComponentsNumber];

            int numberOfLines1 = 0, numberOfColumns1 = 0;
            string line1;
            using (var reader = new StreamReader(_singularValues))
            {
                while ((line1 = await reader.ReadLineAsync()) != null)
                {
                    numberOfColumns1 = line1.Split(' ').Length;
                    numberOfLines1++;
                }
            }

            var array2D1 = new string[numberOfLines1, numberOfColumns1];
            numberOfLines1 = 0;

            using (var reader = new StreamReader(_singularValues))
            {
                while ((line1 = await reader.ReadLineAsync()) != null)
                {
                    var tempArray1 = line1.Split(' ');
                    for (var i = 0; i < tempArray1.Length; ++i)
                    {
                        array2D1[numberOfLines1, i] = tempArray1[i];
                    }
                    numberOfLines1++;
                }
            }


            for (var i = 0; i < 1; i++)
                for (var j = 0; j < principalComponentsNumber; j++)
                    singularValues1[i, j] = Convert.ToDouble(array2D1[i, j]);

            var singularValues = new double[principalComponentsNumber];
            for (var i = 0; i < principalComponentsNumber; i++)
                singularValues[i] = singularValues1[0, i];

            var eigenvalues = singularValues.Pow(2);
            eigenvalues.Divide(matrixMainConverted.GetLength(0) - 1);

            // заміна principalComponentsNumber-ої компоненти на центровані значення(пікселів) бабуїна
            var matrixMainComponents = matrixMainConvertedCentered.MultiplyByTranspose(eigenVectors.Transpose());
            for (var i = 0; i < matrixMainComponents.GetLength(0); i++)
            {
                arrayWithWatermarkPixelsColorful[i] = matrixMainComponents[i, principalComponentsNumber-1];
                //Matrix_Main_Components[i, 15] = 0;
            }

            // пошук середнього для пікселів бабуїна
            for (var i = 0; i < watermarkPixelsColoredArraySize; i++)
                arrayWithWatermarkPixelsColorful[i] = arrayWithWatermarkPixelsColorful[i] + 126;

            // збереження кольорового зображення після певних перетворень
            int tmpR1 = 0, tmpG1 = 1, tmpB1 = 2;
            for (var i = 0; i < watermarkPixelsColoredArraySize; i += 3)
            {
                stringWithOriginalContainerPixels[tmpR1] = (char)arrayWithWatermarkPixelsColorful[i];
                stringWithOriginalContainerPixels[tmpG1] = (char)arrayWithWatermarkPixelsColorful[i + 1];
                stringWithOriginalContainerPixels[tmpB1] = (char)arrayWithWatermarkPixelsColorful[i + 2];
                tmpR1 += 3;
                tmpG1 += 3;
                tmpB1 += 3;
            }

            var p = 0;
            var decryptedWatermark = new Bitmap(watermarkWidth, watermarkHeight);
            for (var i = 0; i < watermarkHeight; i++)
                for (var j = 0; j < watermarkWidth; j++)
                {
                    var processedPixelR = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelR < 0) processedPixelR = 0;
                    if (processedPixelR > 255) processedPixelR = 255;
                    p++;
                    var processedPixelG = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelG < 0) processedPixelG = 0;
                    if (processedPixelG > 255) processedPixelG = 255;
                    p++;
                    var processedPixelB = Convert.ToInt32(stringWithOriginalContainerPixels[p]);
                    if (processedPixelB < 0) processedPixelB = 0;
                    if (processedPixelB > 255) processedPixelB = 255;
                    p++;
                    decryptedWatermark.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixelR), Convert.ToByte(processedPixelG), Convert.ToByte(processedPixelB)));
                }

            decryptedWatermark.Save(Path.Combine(MainConstants.DecryptedWatermarksPath, $"{fileName}_decrypted.bmp"));

            return decryptedWatermark;
        }

        private static EncryptionResult PrepareEncryptionResult(
            Bitmap inputContainer,
            Bitmap inputKey,
            Bitmap outputContainer
            )
        {
            var colors = CalculateColors(inputContainer);
            var colorsForKey = CalculateColors(inputKey);

            return new EncryptionResult
            {
                InputContainer = inputContainer,
                InputKey = inputKey,
                OutputContainer = outputContainer,
                AverageRedColor = colors.Item1,
                AverageGreenColor = colors.Item2,
                AverageBlueColor = colors.Item3,
                AverageRedColorWatermark = colorsForKey.Item1,
                AverageGreenColorWatermark = colorsForKey.Item2,
                AverageBlueColorWatermark = colorsForKey.Item3,
                ContainerWidth = inputContainer.Width,
                ContainerHeight= inputContainer.Height,
                WatermarkHeight = inputKey.Height,
                WatermarkWidth = inputKey.Width
            };
        }


        private static Tuple<int, int, int> CalculateColors(Bitmap inputContainer)
        {
            var srcData = inputContainer.LockBits(
                new Rectangle(0, 0, inputContainer.Width, inputContainer.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            var totals = new long[] { 0, 0, 0 };

            var width = inputContainer.Width;
            var height = inputContainer.Height;

            unsafe
            {
                var p = (byte*)(void*)Scan0;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        for (var color = 0; color < 3; color++)
                        {
                            var idx = (y * stride) + x * 4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            var avgB = (int)totals[0] / (width * height);
            var avgG = (int)totals[1] / (width * height);
            var avgR = (int)totals[2] / (width * height);

            inputContainer.UnlockBits(srcData);
            return new Tuple<int, int, int>(avgR, avgG, avgB);
        }
    }
}