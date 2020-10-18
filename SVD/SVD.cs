using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Watermarking;
using MainConstants = Constants.Constants;

namespace watermarking
{

    public static class Svd
    {
        public static async Task<EncryptionResult> Encrypt(Bitmap container, Bitmap watermark, string fileName)
        {

            var frameDimension = 4;

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
            for (var i = 0; i < container.Width * 3; i += 3)
                for (var j = 0; j < container.Height; j++)
                {
                    matrixMain[i, j] = stringWithOriginalContainerPixels[tmpR];
                    matrixMain[i + 1, j] = stringWithOriginalContainerPixels[tmpG];
                    matrixMain[i + 2, j] = stringWithOriginalContainerPixels[tmpB];
                    tmpR += 3;
                    tmpG += 3;
                    tmpB += 3;
                }

            // конвертація основної матриці 1536х512 в 16-ти стовпцеву
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
            var _singularValues= singularValues;

            var eigenvalues = singularValues.Pow(2);
            eigenvalues = eigenvalues.Divide(matrixMainConverted.GetLength(0) - 1);

            // пошук середнього для пікселів бабуїна
            int tmpSum = 0;
            for (var i = 0; i < stringWithOriginalWatermarkPixels.Length; i++)
                tmpSum += (int)stringWithOriginalWatermarkPixels[i];
            var tmpAvg = tmpSum / stringWithOriginalWatermarkPixels.Length;

            // заміна 16-ої компоненти на центровані значення(пікселів) бабуїна
            var matrixMainComponents = matrixMainConvertedCentered.DotWithTransposed(eigenVectors.Transpose());

            for (var i = 0; i < matrixMainComponents.GetLength(0); i++)
            {
                matrixMainComponents[i, 15] = stringWithOriginalWatermarkPixels[i] - tmpAvg;
                // Matrix_Main_Components[i, 0] = String_With_watermark_Pixels_Colourful[i] - tmp_avg;
                //Matrix_Main_Components[i, 0] = 0;
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
            var  _containerReconstructed = new Bitmap(container.Height, container.Width);
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
                    _containerReconstructed.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixelR), Convert.ToByte(processedPixelG), Convert.ToByte(processedPixelB)));
                }


            await SaveAllDataToFiles(_containerReconstructed, fileName, _eigenVectors, _singularValues);

            return PrepareEncryptionResult(container, watermark, _containerReconstructed);
        }

        private static async Task SaveAllDataToFiles(Image containerProcessed, string fileName, double[,] outputKey1, IEnumerable<double> outputKey2)
        {
            var newFileName = $"{fileName}_Container"; 
            containerProcessed.Save(Path.Combine(global::Constants.Constants.ContainersProcessedPath, $"{newFileName}.bmp"));

            await using (var key1Writer = new StreamWriter(Path.Combine(MainConstants.DecryptKeysPath, $"{newFileName}_EigenVectors.txt")))
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


            await using var key2Writer = new StreamWriter(Path.Combine(MainConstants.DecryptKeysPath, $"{newFileName}_SingularValues.txt"));
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
        public static async Task<Bitmap> Decrypt(Bitmap encryptedContainer, string fileName)
        {
            var _eigenVectors= Path.Combine(MainConstants.DecryptKeysPath, $"{fileName}_EigenVectors.txt");
            var _singularValues = Path.Combine(MainConstants.DecryptKeysPath, $"{fileName}_SingularValues.txt");

            #region grey
            //cmode = radioButtonGrayscale.Checked;

            //if (false)
            //{

            //    int frame_dimension = 4;

            //    // зчитування пікселів лени та бабуїна
            //    StringBuilder String_With_Lena_Pixels = new StringBuilder();
            //    for (int x = 0; x < encryptedContainer.Width; x++)
            //        for (int y = 0; y < encryptedContainer.Height; y++)
            //            String_With_Lena_Pixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).ToArgb() & 0x000000ff));

            //    double[] Array_With_watermark_Pixels = new double[16384];

            //    var Matrix_Main = new double[encryptedContainer.Height, encryptedContainer.Width];
            //    int counter = 0;
            //    for (int i = 0; i < encryptedContainer.Height; i++)
            //        for (int j = 0; j < encryptedContainer.Width; j++)
            //        {
            //            Matrix_Main[i, j] = String_With_Lena_Pixels[counter];
            //            counter++;
            //        }

            //    // конвертація основної матриці 512х512 в 16-ти стовпцеву
            //    int columns = frame_dimension * frame_dimension;
            //    int rows = (encryptedContainer.Width / frame_dimension) * (encryptedContainer.Height / frame_dimension);
            //    var Matrix_Main_Converted = new double[rows, columns];

            //    int k1 = 0, l1 = 0, k_max = encryptedContainer.Height, l_max = encryptedContainer.Width;

            //    int number_of_squares = k_max / frame_dimension;
            //    for (int i = 0; i < rows; i++)
            //        for (int j = 0; j < columns; j++)
            //        {
            //            //Matrix_Main[i, j] = (double)((byte)encryptedContainer.GetPixel(((i * frame) / (int)(Math.Sqrt(rows * col))) * frame + j / frame, (i % ((int)(Math.Sqrt(rows * col)) / frame)) * frame + j % frame).ToArgb() & 0x000000ff);
            //            k1 = j / frame_dimension + (i / number_of_squares) * frame_dimension;
            //            l1 = j % frame_dimension + (i % number_of_squares) * frame_dimension;
            //            Matrix_Main_Converted[i, j] = Matrix_Main[k1, l1];
            //        }

            //    // центрування конвертованої матриці
            //    double[] Column_Mean = Matrix_Main_Converted.Mean(0);
            //    var Matrix_Main_Converted_Centered = Matrix_Main_Converted.Subtract(Column_Mean, 0);

            //    // дешифрування
            //    // зчитування ключів
            //    // перше eigen vectors
            //    //string input = @"..\\..\\..\\eigenvectors.txt";

            //    double[,] eigenvectors = new double[16, 16];
            //    String[,] array2D;
            //    int numberOfLines = 0, numberOfColumns = 0;
            //    string line;
            //    System.IO.StreamReader sr = new System.IO.StreamReader(input);

            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        numberOfColumns = line.Split(' ').Length;
            //        numberOfLines++;
            //    }
            //    sr.Close();

            //    array2D = new String[numberOfLines, numberOfColumns];
            //    numberOfLines = 0;

            //    sr = new System.IO.StreamReader(input);
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        String[] tempArray = line.Split(' ');
            //        for (int i = 0; i < tempArray.Length; ++i)
            //        {
            //            array2D[numberOfLines, i] = tempArray[i];
            //        }
            //        numberOfLines++;
            //    }

            //    for (int i = 0; i < 16; i++)
            //        for (int j = 0; j < 16; j++)
            //            eigenvectors[i, j] = Convert.ToDouble(array2D[i, j]);

            //    // потім Singularvalues
            //    //string input1 = @"..\\..\\..\\Singularvalues.txt";

            //    double[,] Singularvalues1 = new double[1, 16];

            //    String[,] array2D1;
            //    int numberOfLines1 = 0, numberOfColumns1 = 0;
            //    string line1;
            //    System.IO.StreamReader sr1 = new System.IO.StreamReader(input1);

            //    while ((line1 = sr1.ReadLine()) != null)
            //    {
            //        numberOfColumns1 = line1.Split(' ').Length;
            //        numberOfLines1++;
            //    }
            //    sr1.Close();

            //    array2D1 = new String[numberOfLines1, numberOfColumns1];
            //    numberOfLines1 = 0;

            //    sr1 = new System.IO.StreamReader(input1);
            //    while ((line1 = sr1.ReadLine()) != null)
            //    {
            //        String[] tempArray1 = line1.Split(' ');
            //        for (int i = 0; i < tempArray1.Length; ++i)
            //        {
            //            array2D1[numberOfLines1, i] = tempArray1[i];
            //        }
            //        numberOfLines1++;
            //    }

            //    for (int i = 0; i < 1; i++)
            //        for (int j = 0; j < 16; j++)
            //            Singularvalues1[i, j] = Convert.ToDouble(array2D1[i, j]);

            //    double[] Singularvalues = new double[16];
            //    for (int i = 0; i < 16; i++)
            //        Singularvalues[i] = Singularvalues1[0, i];


            //    // заміна 16-ої компоненти на центровані значення(пікселів) бабуїна
            //    var Matrix_Main_Components = Matrix_Main_Converted_Centered.MultiplyByTranspose(eigenvectors.Transpose());
            //    for (int i = 0; i < Matrix_Main_Components.GetLength(0); i++)
            //    {
            //        Array_With_watermark_Pixels[i] = Matrix_Main_Components[i, 15];
            //        //Matrix_Main_Components[i, 15] = 0;
            //    }

            //    // пошук середнього для пікселів бабуїна
            //    for (int i = 0; i < 16384; i++)
            //        Array_With_watermark_Pixels[i] = Array_With_watermark_Pixels[i] + 113;

            //    // збереження чорнобілого зображення після певних перетворень
            //    int tmp = 0;
            //    for (int i = 0; i < 16384; i++)
            //    {
            //        String_With_Lena_Pixels[tmp] = (char)Array_With_watermark_Pixels[i];
            //        tmp++;
            //    }

            //    int p = 0;
            //    decryptedWatermark = new Bitmap(BaboonOriginal128.Height, BaboonOriginal128.Width);
            //    int processedPixel;
            //    for (int i = 0; i < BaboonOriginal128.Width; i++)
            //        for (int j = 0; j < BaboonOriginal128.Height; j++)
            //        {
            //            processedPixel = Convert.ToInt32(String_With_Lena_Pixels[p]);
            //            if (processedPixel < 0) processedPixel = 0;
            //            if (processedPixel > 255) processedPixel = 0;
            //            decryptedWatermark.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixel), Convert.ToByte(processedPixel), Convert.ToByte(processedPixel)));
            //            p++;
            //        }


            //    decryptedWatermark.Save("..\\..\\..\\decryptedWatermark.bmp");

            //    StringBuilder psnr = new StringBuilder();
            //    psnr.Append(Convert.ToString(ComparePSNR(BaboonOriginal128, decryptedWatermark)));
            //    buttonShowDecryptResult.Show();
            //}

            #endregion

            const int frameDimension = 4;

            // зчитування пікселів лени та бабуїна
            var stringWithOriginalContainerPixels = new StringBuilder();
            for (var x = 0; x < encryptedContainer.Width; x++)
                for (var y = 0; y < encryptedContainer.Height; y++)
                {
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).R));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).G));
                    stringWithOriginalContainerPixels.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).B));
                }

            var arrayWithWatermarkPixelsColorful = new double[49152];

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

            // конвертація основної матриці 1536х512 в 16-ти стовпцеву
            const int columns = frameDimension * frameDimension;
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

            var eigenVectors = new double[16, 16];
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

            for (var i = 0; i < 16; i++)
                for (var j = 0; j < 16; j++)
                    eigenVectors[i, j] = Convert.ToDouble(array2D[i, j]);


            var singularValues1 = new double[1, 16];

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
                for (var j = 0; j < 16; j++)
                    singularValues1[i, j] = Convert.ToDouble(array2D1[i, j]);

            var singularValues = new double[16];
            for (var i = 0; i < 16; i++)
                singularValues[i] = singularValues1[0, i];

            var eigenvalues = singularValues.Pow(2);
            eigenvalues.Divide(matrixMainConverted.GetLength(0) - 1);

            // заміна 16-ої компоненти на центровані значення(пікселів) бабуїна
            var matrixMainComponents = matrixMainConvertedCentered.MultiplyByTranspose(eigenVectors.Transpose());
            for (var i = 0; i < matrixMainComponents.GetLength(0); i++)
            {
                arrayWithWatermarkPixelsColorful[i] = matrixMainComponents[i, 15];
                //Matrix_Main_Components[i, 15] = 0;
            }

            // пошук середнього для пікселів бабуїна
            for (var i = 0; i < 49152; i++)
                arrayWithWatermarkPixelsColorful[i] = arrayWithWatermarkPixelsColorful[i] + 126;

            // збереження кольорового зображення після певних перетворень
            int tmpR1 = 0, tmpG1 = 1, tmpB1 = 2;
            for (var i = 0; i < 49152; i += 3)
            {
                stringWithOriginalContainerPixels[tmpR1] = (char)arrayWithWatermarkPixelsColorful[i];
                stringWithOriginalContainerPixels[tmpG1] = (char)arrayWithWatermarkPixelsColorful[i + 1];
                stringWithOriginalContainerPixels[tmpB1] = (char)arrayWithWatermarkPixelsColorful[i + 2];
                tmpR1 += 3;
                tmpG1 += 3;
                tmpB1 += 3;
            }

            var p = 0;
            var decryptedWatermark = new Bitmap(128, 128);
            for (var i = 0; i < 128; i++)
                for (var j = 0; j < 128; j++)
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
            //Bitmap outputKey
            )
        {
            return new EncryptionResult
            {
                InputContainer = inputContainer,
                InputKey = inputKey,
                OutputContainer = outputContainer,
            };
        }
    }
}