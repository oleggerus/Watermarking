using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace watermarking
{
    public static class SVD
    {
        private static Bitmap container_Reconstructed;
        private static Bitmap watermark_Reconstructed;
        private static double[,] key1;
        private static double[] key2;
        private static string input;
        private static string input1;

        public static void Encrypt(Bitmap container, Bitmap watermark)
        {

            int frame_dimension = 4;

            // зчитування пікселів лени та бабуїна
            StringBuilder String_With_Lena_Pixels_Colourful = new StringBuilder();
            for (int x = 0; x < container.Width; x++)
            {
                for (int y = 0; y < container.Height; y++)
                {
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(container.GetPixel(x, y).R));
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(container.GetPixel(x, y).G));
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(container.GetPixel(x, y).B));
                }
            }

            StringBuilder String_With_watermark_Pixels_Colourful = new StringBuilder();
            for (int i = 0; i < watermark.Height; i++)
                for (int j = 0; j < watermark.Width; j++)
                {
                    String_With_watermark_Pixels_Colourful.Append(Convert.ToChar(watermark.GetPixel(i, j).R));
                    String_With_watermark_Pixels_Colourful.Append(Convert.ToChar(watermark.GetPixel(i, j).G));
                    String_With_watermark_Pixels_Colourful.Append(Convert.ToChar(watermark.GetPixel(i, j).B));
                }

            var Matrix_Main = new double[container.Height * 3, container.Width];

            int tmpR = 0, tmpG = 1, tmpB = 2;
            for (int i = 0; i < container.Width * 3; i += 3)
                for (int j = 0; j < container.Height; j++)
                {
                    Matrix_Main[i, j] = String_With_Lena_Pixels_Colourful[tmpR];
                    Matrix_Main[i + 1, j] = String_With_Lena_Pixels_Colourful[tmpG];
                    Matrix_Main[i + 2, j] = String_With_Lena_Pixels_Colourful[tmpB];
                    tmpR += 3;
                    tmpG += 3;
                    tmpB += 3;
                }

            // конвертація основної матриці 1536х512 в 16-ти стовпцеву
            int columns = frame_dimension * frame_dimension;
            int rows = ((container.Width / frame_dimension) * (container.Height / frame_dimension)) * 3;
            var Matrix_Main_Converted = new double[rows, columns];

            int k1 = 0, l1 = 0, k_max = container.Height, l_max = container.Width;
            int number_of_squares = k_max / frame_dimension;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    //Matrix_Main[i, j] = (double)((byte)container.GetPixel(((i * frame) / (int)(Math.Sqrt(rows * col))) * frame + j / frame, (i % ((int)(Math.Sqrt(rows * col)) / frame)) * frame + j % frame).ToArgb() & 0x000000ff);
                    k1 = j / frame_dimension + (i / number_of_squares) * frame_dimension;
                    l1 = j % frame_dimension + (i % number_of_squares) * frame_dimension;
                    Matrix_Main_Converted[i, j] = Matrix_Main[k1, l1];
                }

            // центрування конвертованої матриці
            double[] Column_Mean = Matrix_Main_Converted.Mean(0);
            var Matrix_Main_Converted_Centered = Matrix_Main_Converted.Subtract(Column_Mean, 0);

            //SVD
            SingularValueDecomposition svd = new SingularValueDecomposition(Matrix_Main_Converted_Centered);
            var eigenvectors = svd.RightSingularVectors;
            var Singularvalues = svd.Diagonal;
            key1 = eigenvectors;
            key2 = Singularvalues;

            double[] eigenvalues = Singularvalues.Pow(2);
            eigenvalues = eigenvalues.Divide(Matrix_Main_Converted.GetLength(0) - 1);

            // пошук середнього для пікселів бабуїна
            int tmp_sum = 0, tmp_avg = 0;
            for (int i = 0; i < String_With_watermark_Pixels_Colourful.Length; i++)
                tmp_sum += (int)String_With_watermark_Pixels_Colourful[i];
            tmp_avg = tmp_sum / String_With_watermark_Pixels_Colourful.Length;

            // заміна 16-ої компоненти на центровані значення(пікселів) бабуїна
            var Matrix_Main_Components = Matrix_Main_Converted_Centered.MultiplyByTranspose(eigenvectors.Transpose());

            for (int i = 0; i < Matrix_Main_Components.GetLength(0); i++)
            {
                //Matrix_Main_Components[i, 15] = String_With_watermark_Pixels_Colourful_Centered[i];
                Matrix_Main_Components[i, 0] = String_With_watermark_Pixels_Colourful[i] - tmp_avg;
                //Matrix_Main_Components[i, 0] = 0;
            }

            // реконструкція конвертованої матриці
            var Back_To_Matrix_Main_Converted_Centered = Matrix_Main_Components.DotWithTransposed(eigenvectors);
            double[,] Matrix_Reconstructed = Back_To_Matrix_Main_Converted_Centered.Add(Column_Mean, 0);

            // повна реконструкція матриці
            var Matrix_Main_Reconstructed_Fully = new double[container.Height * 3, container.Width];
            int k2 = 0, l2 = 0;
            for (int i = 0; i < Matrix_Reconstructed.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix_Reconstructed.GetLength(1); j++)
                {
                    k2 = j / frame_dimension + (i / number_of_squares) * frame_dimension;
                    l2 = j % frame_dimension + (i % number_of_squares) * frame_dimension;
                    Matrix_Main_Reconstructed_Fully[k2, l2] = Matrix_Reconstructed[i, j];
                }
            }

            // вивід кольорового зображення після певних перетворень
            int tmpR1 = 0, tmpG1 = 1, tmpB1 = 2;
            for (int i = 0; i < container.Width * 3; i += 3)
                for (int j = 0; j < container.Height; j++)
                {
                    String_With_Lena_Pixels_Colourful[tmpR1] = (char)Matrix_Main_Reconstructed_Fully[i, j];
                    String_With_Lena_Pixels_Colourful[tmpG1] = (char)Matrix_Main_Reconstructed_Fully[i + 1, j];
                    String_With_Lena_Pixels_Colourful[tmpB1] = (char)Matrix_Main_Reconstructed_Fully[i + 2, j];
                    tmpR1 += 3;
                    tmpG1 += 3;
                    tmpB1 += 3;
                }

            int p = 0;
            container_Reconstructed = new Bitmap(container.Height, container.Width);
            int processedPixelR;
            int processedPixelG;
            int processedPixelB;
            for (int i = 0; i < container.Width; i++)
                for (int j = 0; j < container.Height; j++)
                {
                    processedPixelR = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelR < 0) processedPixelR = 0;
                    if (processedPixelR > 255) processedPixelR = 255;
                    p++;
                    processedPixelG = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelG < 0) processedPixelG = 255;
                    if (processedPixelG > 255) processedPixelG = 0;
                    p++;
                    processedPixelB = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelB < 0) processedPixelB = 0;
                    if (processedPixelB > 255) processedPixelB = 255;
                    p++;
                    container_Reconstructed.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixelR), Convert.ToByte(processedPixelG), Convert.ToByte(processedPixelB)));
                }
            container_Reconstructed.Save("..\\..\\..\\Lena_Reconstructed_Colourful.bmp");
        }

        public static Bitmap Decrypt(Bitmap encryptedContainer)
        {
            input = @"..\\..\\..\\eigenvectors.txt";
            input1 = @"..\\..\\..\\Singularvalues.txt";

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

            int frame_dimension = 4;

            // зчитування пікселів лени та бабуїна
            StringBuilder String_With_Lena_Pixels_Colourful = new StringBuilder();
            for (int x = 0; x < encryptedContainer.Width; x++)
                for (int y = 0; y < encryptedContainer.Height; y++)
                {
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).R));
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).G));
                    String_With_Lena_Pixels_Colourful.Append(Convert.ToChar(encryptedContainer.GetPixel(x, y).B));
                }

            double[] Array_With_watermark_Pixels_Colourful = new double[49152];

            var Matrix_Main = new double[encryptedContainer.Height * 3, encryptedContainer.Width];

            int tmpR = 0, tmpG = 1, tmpB = 2;
            for (int i = 0; i < encryptedContainer.Width * 3; i += 3)
                for (int j = 0; j < encryptedContainer.Height; j++)
                {
                    Matrix_Main[i, j] = String_With_Lena_Pixels_Colourful[tmpR];
                    Matrix_Main[i + 1, j] = String_With_Lena_Pixels_Colourful[tmpG];
                    Matrix_Main[i + 2, j] = String_With_Lena_Pixels_Colourful[tmpB];
                    tmpR += 3;
                    tmpG += 3;
                    tmpB += 3;
                }

            // конвертація основної матриці 1536х512 в 16-ти стовпцеву
            int columns = frame_dimension * frame_dimension;
            int rows = ((encryptedContainer.Width / frame_dimension) * (encryptedContainer.Height / frame_dimension)) * 3;
            var Matrix_Main_Converted = new double[rows, columns];

            int k1 = 0, l1 = 0, k_max = encryptedContainer.Height, l_max = encryptedContainer.Width;
            int number_of_squares = k_max / frame_dimension;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    //Matrix_Main[i, j] = (double)((byte)encryptedContainer.GetPixel(((i * frame) / (int)(Math.Sqrt(rows * col))) * frame + j / frame, (i % ((int)(Math.Sqrt(rows * col)) / frame)) * frame + j % frame).ToArgb() & 0x000000ff);
                    k1 = j / frame_dimension + (i / number_of_squares) * frame_dimension;
                    l1 = j % frame_dimension + (i % number_of_squares) * frame_dimension;
                    Matrix_Main_Converted[i, j] = Matrix_Main[k1, l1];
                }

            // центрування конвертованої матриці
            double[] Column_Mean = Matrix_Main_Converted.Mean(0);
            var Matrix_Main_Converted_Centered = Matrix_Main_Converted.Subtract(Column_Mean, 0);

            // дешифрування
            // зчитування ключів
            // перше eigen vectors
            //string input = @"..\\..\\..\\eigenvectorsC.txt";

            double[,] eigenvectors = new double[16, 16];
            String[,] array2D;
            int numberOfLines = 0, numberOfColumns = 0;
            string line;
            System.IO.StreamReader sr = new System.IO.StreamReader(input);

            while ((line = sr.ReadLine()) != null)
            {
                numberOfColumns = line.Split(' ').Length;
                numberOfLines++;
            }
            sr.Close();

            array2D = new String[numberOfLines, numberOfColumns];
            numberOfLines = 0;

            sr = new System.IO.StreamReader(input);
            while ((line = sr.ReadLine()) != null)
            {
                String[] tempArray = line.Split(' ');
                for (int i = 0; i < tempArray.Length; ++i)
                {
                    array2D[numberOfLines, i] = tempArray[i];
                }
                numberOfLines++;
            }

            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    eigenvectors[i, j] = Convert.ToDouble(array2D[i, j]);

            // потім Singularvalues
            //string input1 = @"..\\..\\..\\SingularvaluesC.txt";

            double[,] Singularvalues1 = new double[1, 16];

            String[,] array2D1;
            int numberOfLines1 = 0, numberOfColumns1 = 0;
            string line1;
            System.IO.StreamReader sr1 = new System.IO.StreamReader(input1);

            while ((line1 = sr1.ReadLine()) != null)
            {
                numberOfColumns1 = line1.Split(' ').Length;
                numberOfLines1++;
            }
            sr1.Close();

            array2D1 = new String[numberOfLines1, numberOfColumns1];
            numberOfLines1 = 0;

            sr1 = new System.IO.StreamReader(input1);
            while ((line1 = sr1.ReadLine()) != null)
            {
                String[] tempArray1 = line1.Split(' ');
                for (int i = 0; i < tempArray1.Length; ++i)
                {
                    array2D1[numberOfLines1, i] = tempArray1[i];
                }
                numberOfLines1++;
            }

            for (int i = 0; i < 1; i++)
                for (int j = 0; j < 16; j++)
                    Singularvalues1[i, j] = Convert.ToDouble(array2D1[i, j]);

            double[] Singularvalues = new double[16];
            for (int i = 0; i < 16; i++)
                Singularvalues[i] = Singularvalues1[0, i];

            double[] eigenvalues = Singularvalues.Pow(2);
            eigenvalues = eigenvalues.Divide(Matrix_Main_Converted.GetLength(0) - 1);

            // заміна 16-ої компоненти на центровані значення(пікселів) бабуїна
            var Matrix_Main_Components = Matrix_Main_Converted_Centered.MultiplyByTranspose(eigenvectors.Transpose());
            for (int i = 0; i < Matrix_Main_Components.GetLength(0); i++)
            {
                Array_With_watermark_Pixels_Colourful[i] = Matrix_Main_Components[i, 15];
                //Matrix_Main_Components[i, 15] = 0;
            }

            // пошук середнього для пікселів бабуїна
            for (int i = 0; i < 49152; i++)
                Array_With_watermark_Pixels_Colourful[i] = Array_With_watermark_Pixels_Colourful[i] + 126;

            // збереження кольорового зображення після певних перетворень
            int tmpR1 = 0, tmpG1 = 1, tmpB1 = 2;
            for (int i = 0; i < 49152; i += 3)
            {
                String_With_Lena_Pixels_Colourful[tmpR1] = (char)Array_With_watermark_Pixels_Colourful[i];
                String_With_Lena_Pixels_Colourful[tmpG1] = (char)Array_With_watermark_Pixels_Colourful[i + 1];
                String_With_Lena_Pixels_Colourful[tmpB1] = (char)Array_With_watermark_Pixels_Colourful[i + 2];
                tmpR1 += 3;
                tmpG1 += 3;
                tmpB1 += 3;
            }

            int p = 0;
            var decryptedWatermark = new Bitmap(128, 128);
            int processedPixelR;
            int processedPixelG;
            int processedPixelB;
            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 128; j++)
                {
                    processedPixelR = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelR < 0) processedPixelR = 0;
                    if (processedPixelR > 255) processedPixelR = 255;
                    p++;
                    processedPixelG = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelG < 0) processedPixelG = 0;
                    if (processedPixelG > 255) processedPixelG = 255;
                    p++;
                    processedPixelB = Convert.ToInt32(String_With_Lena_Pixels_Colourful[p]);
                    if (processedPixelB < 0) processedPixelB = 0;
                    if (processedPixelB > 255) processedPixelB = 255;
                    p++;
                    decryptedWatermark.SetPixel(i, j, Color.FromArgb(Convert.ToByte(processedPixelR), Convert.ToByte(processedPixelG), Convert.ToByte(processedPixelB)));
                }

            decryptedWatermark.Save("..\\..\\..\\decryptedWatermark.bmp");

            return decryptedWatermark;
        }

    }
}