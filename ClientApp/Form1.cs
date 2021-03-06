﻿using Algorithm;
using DAL;
using DAL.Services;
using SVD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        public string ContainerFileName { get; set; }
        public string WatermarkFileName { get; set; }

        public string OriginalFileName { get; set; }
        public ProcessingResult CurrentEncryptionResult { get; set; }

        public Bitmap OriginalContainer { get; set; }
        public Bitmap WatermarkedContainer { get; set; }

        public bool Executed { get; set; }

        public Form1()
        {
            InitializeComponent();
            displayOriginalRadionBtn.Checked = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'watermarkingDataSet.WatermarkingResults' table. You can move, or remove it, as needed.
            this.watermarkingResultsTableAdapter.Fill(this.watermarkingDataSet.WatermarkingResults);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            ContainerFileName = openFileDialog1.FileName;
            if (!string.IsNullOrWhiteSpace(ContainerFileName) && ContainerFileName != "openFileDialog1")
            {
                ContainerPictureBox.Image = Image.FromFile(openFileDialog1.FileName);
                OriginalFileName = $"{Path.GetFileNameWithoutExtension(ContainerFileName)}_{Path.GetFileNameWithoutExtension(WatermarkFileName)}";
                if (!string.IsNullOrWhiteSpace(WatermarkFileName) && WatermarkFileName != "openFileDialog2")
                    OriginalContainer = new Bitmap(ContainerFileName ?? string.Empty);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            WatermarkFileName = openFileDialog2.FileName;
            if (!string.IsNullOrWhiteSpace(WatermarkFileName) && WatermarkFileName != "openFileDialog2")
            {
                WatermarkPictureBox.Image = Image.FromFile(openFileDialog2.FileName);
                if (!string.IsNullOrWhiteSpace(ContainerFileName) && ContainerFileName != "openFileDialog1")
                    OriginalFileName = $"{Path.GetFileNameWithoutExtension(ContainerFileName)}_{Path.GetFileNameWithoutExtension(WatermarkFileName)}";
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WatermarkFileName) || WatermarkFileName == "openFileDialog2" || string.IsNullOrWhiteSpace(ContainerFileName) || ContainerFileName == "openFileDialog1")
            {
                return;
            }

            if (!Executed)
            {
                OriginalFileName = $"{Path.GetFileNameWithoutExtension(ContainerFileName)}_{Path.GetFileNameWithoutExtension(WatermarkFileName)}";
                OriginalContainer = new Bitmap(ContainerFileName ?? string.Empty);
            }

            var watermark = new Bitmap(WatermarkFileName ?? string.Empty);
            CurrentEncryptionResult = await Executor.HandleEncryption(OriginalContainer, watermark, OriginalFileName);

            EditedContainerPictureBox.Image = CurrentEncryptionResult.ContainerWithWatermark;
            WatermarkedContainer = CurrentEncryptionResult.ContainerWithWatermark;

            var decryptionResult = await Executor.Decrypt(OriginalFileName, watermark);
            EditedWatermarkPictureBox.Image = decryptionResult.ExtractedWatermark;

            SetLabelsVisibility(CurrentEncryptionResult, decryptionResult);
            Executed = true;

            //var ssim1 = SSIM.SsimIndex(OriginalContainer, WatermarkedContainer);
            //var mse1 = Helpers.CalculateMse(OriginalContainer, WatermarkedContainer);

            //var ssim2 = SSIM.SsimIndex(watermark, decryptionResult.ExtractedWatermark);
            //var mse2 = Helpers.CalculateMse(watermark, decryptionResult.ExtractedWatermark);

            var insertModel = Factory.PrepareResultModel(Path.GetFileNameWithoutExtension(ContainerFileName), Path.GetFileNameWithoutExtension(WatermarkFileName), CurrentEncryptionResult.Time,
             decryptionResult.Time, CurrentEncryptionResult.Psnr, decryptionResult.Psnr, CurrentEncryptionResult.Mse, decryptionResult.Mse, (int)BrightnessOriginalUpDown.Value, (int)ContrastOriginalUpDown.Value, (int)NoiseOriginalUpDown.Value,
             CurrentEncryptionResult.AverageRedColor, CurrentEncryptionResult.AverageGreenColor, CurrentEncryptionResult.AverageBlueColor,
             CurrentEncryptionResult.AverageRedColorWatermark, CurrentEncryptionResult.AverageGreenColorWatermark, CurrentEncryptionResult.AverageBlueColorWatermark,
             CurrentEncryptionResult.ContainerWidth, CurrentEncryptionResult.ContainerHeight,
             CurrentEncryptionResult.WatermarkWidth, CurrentEncryptionResult.WatermarkHeight);
            insertModel.Mode = (int)WatermarkingMode.Single;
            await DalService.InsertResult(insertModel);
        }


        private void SetLabelsVisibility(ProcessingResult encryptionResult, ProcessingResult decryptionResult)
        {

            label35.Text = Math.Round(encryptionResult.Mse, 2).ToString();
            label36.Text = Math.Round(decryptionResult.Mse, 2).ToString();

            label12.Text = Math.Round(decryptionResult.Psnr, 2).ToString();
            label13.Text = Math.Round(encryptionResult.Psnr, 2).ToString();

            label14.Text = $@"{encryptionResult.WatermarkHeight}x{encryptionResult.WatermarkWidth}";
            label15.Text = $@"{encryptionResult.ContainerHeight}x{encryptionResult.ContainerWidth}";
            label16.Text = encryptionResult.AverageBlueColor.ToString();
            label17.Text = encryptionResult.AverageGreenColor.ToString();
            label18.Text = encryptionResult.AverageRedColor.ToString();

            label19.Text = encryptionResult.AverageBlueColorWatermark.ToString();
            label20.Text = encryptionResult.AverageGreenColorWatermark.ToString();
            label21.Text = encryptionResult.AverageRedColorWatermark.ToString();


            var editedContainer = Helpers.CalculateColors(WatermarkedContainer);
            label25.Text = editedContainer.Item3.ToString();
            label26.Text = editedContainer.Item2.ToString();
            label27.Text = editedContainer.Item1.ToString();

            var extractedWatermark = Helpers.CalculateColors((Bitmap)EditedWatermarkPictureBox.Image);
            label1.Text = extractedWatermark.Item3.ToString();
            label2.Text = extractedWatermark.Item2.ToString();
            label10.Text = extractedWatermark.Item1.ToString();

            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label12.Visible = true;
            label13.Visible = true;
            label14.Visible = true;
            label15.Visible = true;
            label16.Visible = true;
            label17.Visible = true;
            label18.Visible = true;
            label19.Visible = true;
            label20.Visible = true;
            label21.Visible = true;
            label22.Visible = true;
            label23.Visible = true;
            label24.Visible = true;
            label25.Visible = true;
            label26.Visible = true;
            label27.Visible = true;
            label28.Visible = true;
            label29.Visible = true;
            label30.Visible = true;
            label31.Visible = true;
            label32.Visible = true;
            label11.Visible = true;
            label10.Visible = true;
            label2.Visible = true;
            label1.Visible = true;

            label33.Visible = true;
            label34.Visible = true;
            label35.Visible = true;
            label36.Visible = true;

            ExtractBtn.Visible = true;
            NoiseBtn.Visible = true;
            ContrastBtn.Visible = true;
            BrightnessBtn.Visible = true;
            ResizeBtn.Visible = true;
            NoiseUpDown.Visible = true;
            ContrastUpDown.Visible = true;
            BrightnessUpDown.Visible = true;

            NoiseOriginalBtn.Visible = true;
            ContrastOriginalBtn.Visible = true;
            BrightnessOriginalBtn.Visible = true;
            ResizeOriginalBtn.Visible = true;
            NoiseOriginalUpDown.Visible = true;
            ContrastOriginalUpDown.Visible = true;
            BrightnessOriginalUpDown.Visible = true;
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                await PopulateGridWithFullData();
            }
            if (tabControl1.SelectedIndex == 3)
            {
                await DisplayEncryptionSizeChart();
            }
            if (tabControl1.SelectedIndex == 4)
            {
                await DisplayDecryptionSizeChart();
            }
            if (tabControl1.SelectedIndex == 5)
            {
                await DisplayEffectsChart();
            }
        }

        private async Task PopulateGridWithFullData()
        {
            if (displayOriginalRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = await DalService.GetAllResultByMode(new List<int>
                {
                    (int) WatermarkingMode.AllToAll, (int) WatermarkingMode.OneKeyToAllContainers,
                    (int) WatermarkingMode.OneContainerToAllKeys
                });

                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, "-", "-", "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayContrastContainerRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneContainerToAllKeysWithContrast))
                    .Where(x => x.Brightness.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, item.Contrast, "-", "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayBrightnessContainerRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneContainerToAllKeysWithBrightness))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, "-", item.Brightness, "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
                GridViewTab3.Refresh();
            }
            else if (displayNoiseContainerRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneContainerToAllKeysWithNoise))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, "-", "-", item.Noise,
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
                GridViewTab3.Refresh();
            }
            else if (displayContrastWatermarkRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneKeyToAllContainersWithContrast))
                    .Where(x => x.Brightness.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, item.Contrast, "-", "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayBrightnessWatermarkRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneKeyToAllContainersWithBrightness))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, "-", item.Brightness, "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
                GridViewTab3.Refresh();
            }
            else if (displayNoiseWatermarkRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode((int)WatermarkingMode.OneKeyToAllContainersWithNoise))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName.Substring(0, item.ContainerFileName.IndexOf('_')), item.KeyFileName, "-", "-", item.Noise,
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        Math.Round(item.EncryptionMse.GetValueOrDefault(), 2), Math.Round(item.DecryptionMse.GetValueOrDefault(), 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
                GridViewTab3.Refresh();
            }


        }

        private async void displayContrastRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();

        }

        private async void displayOriginalRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();

        }

        private async void displayBrightnessRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();

        }

        private async void displayContrastWatermarkRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();

        }

        private async void displayBrightnessWatermarkRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();
        }

        private void NoiseBtn_Click(object sender, EventArgs e)
        {
            WatermarkedContainer = Helpers.SetNoise(WatermarkedContainer, (int)NoiseUpDown.Value);
            EditedContainerPictureBox.Image = WatermarkedContainer;
        }

        private void ContrastBtn_Click(object sender, EventArgs e)
        {
            WatermarkedContainer = Helpers.SetContrast(WatermarkedContainer, (int)ContrastUpDown.Value);
            EditedContainerPictureBox.Image = WatermarkedContainer;
        }

        private void BrightnessBtn_Click(object sender, EventArgs e)
        {
            WatermarkedContainer = Helpers.SetBrightness(WatermarkedContainer, (int)BrightnessUpDown.Value);
            EditedContainerPictureBox.Image = WatermarkedContainer;
        }

        private async void ExtractBtn_Click(object sender, EventArgs e)
        {
            var watermark = new Bitmap(WatermarkFileName ?? string.Empty);

            var decryptionResult = await Executor.DecryptFromBitmap((Bitmap)(EditedContainerPictureBox.Image), OriginalFileName, watermark);
            EditedWatermarkPictureBox.Image = decryptionResult.ExtractedWatermark;

            SetLabelsVisibility(CurrentEncryptionResult, decryptionResult);
        }

        private void NoiseOriginalBtn_Click(object sender, EventArgs e)
        {
            OriginalContainer = Helpers.SetNoise(WatermarkedContainer, (int)NoiseOriginalUpDown.Value);
            ContainerPictureBox.Image = OriginalContainer;
        }

        private void ContrastOriginalBtn_Click(object sender, EventArgs e)
        {
            OriginalContainer = Helpers.SetContrast(WatermarkedContainer, (int)ContrastOriginalUpDown.Value);
            ContainerPictureBox.Image = OriginalContainer;
        }

        private void BrightnessOriginalBtn_Click(object sender, EventArgs e)
        {
            OriginalContainer = Helpers.SetBrightness(WatermarkedContainer, (int)BrightnessOriginalUpDown.Value);
            ContainerPictureBox.Image = OriginalContainer;
        }

        private void ResizeOriginalBtn_Click(object sender, EventArgs e)
        {
            OriginalContainer = Helpers.ResizeImage(WatermarkedContainer, OriginalContainer.Width / 2, OriginalContainer.Height / 2);
            ContainerPictureBox.Image = OriginalContainer;
        }

        private void ResizeBtn_Click(object sender, EventArgs e)
        {
            WatermarkedContainer = Helpers.ResizeImage(WatermarkedContainer, WatermarkedContainer.Width / 2, WatermarkedContainer.Height / 2);
            EditedContainerPictureBox.Image = WatermarkedContainer;
        }

        private async void displayNoiseContainerRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();

        }

        private async void displayNoiseWatermarkRadionBtn_CheckedChanged(object sender, EventArgs e)
        {
            await PopulateGridWithFullData();
        }


        private async Task DisplayDecryptionSizeChart()
        {

            var rows = (await DalService.GetAllResultByMode((int)WatermarkingMode.Single)).OrderBy(x => x.ContainerWidth).ToList();
            foreach (var row in rows)
            {
                row.ContainerFileName = row.ContainerFileName.Replace("_", "").Replace("64", "").Replace("128", "")
                    .Replace("256", "").Replace("512", "").Replace("1024", "");
            }

            SetPSNRChartsDecryption(rows);
            SetMseChartsDecryption(rows);
        }

        private async Task DisplayEncryptionSizeChart()
        {

            var rows = (await DalService.GetAllResultByMode((int)WatermarkingMode.Single)).OrderBy(x => x.ContainerWidth).ToList();
            foreach (var row in rows)
            {
                row.ContainerFileName = row.ContainerFileName.Replace("_", "").Replace("64", "").Replace("128", "")
                    .Replace("256", "").Replace("512", "").Replace("1024", "");
            }

            SetPSNRCharts(rows);
            SetMseCharts(rows);
        }

        private async Task DisplayEffectsChart()
        {

            var rows = await DalService.GetAllResultByMode(new List<int> { (int)WatermarkingMode.OneContainerToAllKeysWithNoise, (int)WatermarkingMode.OneContainerToAllKeysWithBrightness, (int)WatermarkingMode.OneContainerToAllKeysWithContrast });
            foreach (var row in rows)
            {
                row.ContainerFileName = row.ContainerFileName.Substring(0, row.ContainerFileName.IndexOf('_'));

                row.ContainerFileName = row.ContainerFileName.Replace("_", "").Replace("64", "").Replace("128", "")
                    .Replace("256", "").Replace("512", "").Replace("1024", "");
            }

            #region noise
            var dtNoise = new DataTable();
            dtNoise.Columns.Add("Container Name", typeof(string));
            dtNoise.Columns.Add("Encryption PSNR", typeof(decimal));
            dtNoise.Columns.Add("Noise level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithNoise
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Noise).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Noise))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtNoise.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            encNoiseChart1.Series.Clear();
            encNoiseChart1.DataBindCrossTable(dtNoise.Rows, "Container Name", "Noise level", "Encryption PSNR", "");
            encNoiseChart1.Palette = ChartColorPalette.Berry;
            encNoiseChart1.Titles.Clear();
            encNoiseChart1.Titles.Add("Noise level effect");
            UpdateChart(encNoiseChart1, "PSNR", "Noise level");

            var dtNoiseDec = new DataTable();
            dtNoiseDec.Columns.Add("Container Name", typeof(string));
            dtNoiseDec.Columns.Add("Encryption PSNR", typeof(decimal));
            dtNoiseDec.Columns.Add("Noise level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithNoise
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Noise).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Noise))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtNoiseDec.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            decNoiseChart1.Series.Clear();
            decNoiseChart1.DataBindCrossTable(dtNoiseDec.Rows, "Container Name", "Noise level", "Encryption PSNR", "");
            decNoiseChart1.Palette = ChartColorPalette.Berry;
            decNoiseChart1.Titles.Clear();
            decNoiseChart1.Titles.Add("Noise level effect");
            UpdateChart(decNoiseChart1, "PSNR", "Noise level");
            #endregion

            #region brightness
            var dtBrightness = new DataTable();
            dtBrightness.Columns.Add("Container Name", typeof(string));
            dtBrightness.Columns.Add("Encryption PSNR", typeof(decimal));
            dtBrightness.Columns.Add("Brightness level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithBrightness
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Brightness).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Brightness))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtBrightness.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            encBrightnessChart.Series.Clear();
            encBrightnessChart.DataBindCrossTable(dtBrightness.Rows, "Container Name", "Brightness level", "Encryption PSNR", "");
            encBrightnessChart.Palette = ChartColorPalette.Berry;
            encBrightnessChart.Titles.Clear();
            encBrightnessChart.Titles.Add("Brightness level effect");
            UpdateChart(encBrightnessChart, "PSNR", "Brightness level");

            var dtBrightnessDec = new DataTable();
            dtBrightnessDec.Columns.Add("Container Name", typeof(string));
            dtBrightnessDec.Columns.Add("Encryption PSNR", typeof(decimal));
            dtBrightnessDec.Columns.Add("Brightness level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithBrightness
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Brightness).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Brightness))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtBrightnessDec.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            decBrightnessChart.Series.Clear();
            decBrightnessChart.DataBindCrossTable(dtBrightnessDec.Rows, "Container Name", "Brightness level", "Encryption PSNR", "");
            decBrightnessChart.Palette = ChartColorPalette.Berry;
            decBrightnessChart.Titles.Clear();
            decBrightnessChart.Titles.Add("Brightness level effect");
            UpdateChart(decBrightnessChart, "PSNR", "Brightness level");
            #endregion

            #region contrast

            var dtContrast = new DataTable();
            dtContrast.Columns.Add("Container Name", typeof(string));
            dtContrast.Columns.Add("Encryption PSNR", typeof(decimal));
            dtContrast.Columns.Add("Contrast level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithContrast
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Contrast).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Contrast))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtContrast.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            encContrastChart1.Series.Clear();
            encContrastChart1.DataBindCrossTable(dtContrast.Rows, "Container Name", "Contrast level", "Encryption PSNR", "");
            encContrastChart1.Palette = ChartColorPalette.Berry;
            encContrastChart1.Titles.Clear();
            encContrastChart1.Titles.Add("Contrast level effect");
            UpdateChart(encContrastChart1, "PSNR", "Contrast level");

            var dtContrastDec = new DataTable();
            dtContrastDec.Columns.Add("Container Name", typeof(string));
            dtContrastDec.Columns.Add("Encryption PSNR", typeof(decimal));
            dtContrastDec.Columns.Add("Contrast level", typeof(string));

            foreach (var group in rows.Where(x => x.Mode == (int)WatermarkingMode.OneContainerToAllKeysWithContrast
            && x.WatermarkHeight.GetValueOrDefault() == 128).OrderBy(x => x.Contrast).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.Contrast))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dtContrastDec.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key);
                }
            }

            decContrastChart1.Series.Clear();
            decContrastChart1.DataBindCrossTable(dtContrastDec.Rows, "Container Name", "Contrast level", "Encryption PSNR", "");
            decContrastChart1.Palette = ChartColorPalette.Berry;
            decContrastChart1.Titles.Clear();
            decContrastChart1.Titles.Add("Contrast level effect");
            UpdateChart(decContrastChart1, "PSNR", "Contrast level");

            #endregion
        }


        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void SetPSNRCharts(List<WatermarkingResults> rows)
        {
            var dt64 = new DataTable();
            dt64.Columns.Add("Container Name", typeof(string));
            dt64.Columns.Add("Encryption PSNR", typeof(decimal));
            dt64.Columns.Add("Size", typeof(int));

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 64).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt64.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key == 128 ? 0 : groupBySize.Key == 256 ? 1 : groupBySize.Key == 512 ? 2 : 3);
                }
            }

            size64Chart.Series.Clear();
            size64Chart.DataBindCrossTable(dt64.Rows, "Container Name", "Size", "Encryption PSNR", "");
            size64Chart.Palette = ChartColorPalette.Berry;
            size64Chart.Titles.Clear();
            size64Chart.Titles.Add("64x64 watermark results");
            UpdateChart(size64Chart, "PSNR");


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var dt128 = new DataTable();
            dt128.Columns.Add("Container Name", typeof(string));
            dt128.Columns.Add("Encryption PSNR", typeof(decimal));
            dt128.Columns.Add("Size", typeof(int));
            size128Chart.Series.Clear();

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 128)
                .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt128.Rows.Add(group.Key, Math.Round(max, 2),
                    groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }

            size128Chart.DataBindCrossTable(dt128.Rows, "Container Name", "Size", "Encryption PSNR", "");
            size128Chart.Palette = ChartColorPalette.SeaGreen;
            size128Chart.Titles.Clear();
            size128Chart.Titles.Add("128x128 watermark results");

            UpdateChart(size128Chart, "PSNR");


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var dt256 = new DataTable();
            dt256.Columns.Add("Container Name", typeof(string));
            dt256.Columns.Add("Encryption PSNR", typeof(decimal));
            dt256.Columns.Add("Size", typeof(int));
            size256Chart.Titles.Clear();
            size256Chart.Titles.Add("256x256 watermark results");
            size256Chart.Series.Clear();


            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 256)
                                                                 .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt256.Rows.Add(group.Key, Math.Round(max), groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }
            size256Chart.DataBindCrossTable(dt256.Rows, "Container Name", "Size", "Encryption PSNR", "");
            UpdateChart(size256Chart, "PSNR");

        }

        private void SetMseCharts(List<WatermarkingResults> rows)
        {
            var mse64 = new DataTable();
            mse64.Columns.Add("Container Name", typeof(string));
            mse64.Columns.Add("Encryption MSE", typeof(decimal));
            mse64.Columns.Add("Size", typeof(int));

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 64).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse64.Rows.Add(group.Key, Math.Round(max, 2),
                           groupBySize.Key == 128 ? 0 : groupBySize.Key == 256 ? 1 : groupBySize.Key == 512 ? 2 : 3);
                }
            }

            mseChart1.Series.Clear();
            mseChart1.DataBindCrossTable(mse64.Rows, "Container Name", "Size", "Encryption MSE", "");
            mseChart1.Palette = ChartColorPalette.Berry;
            mseChart1.Titles.Clear();
            mseChart1.Titles.Add("64x64 watermark results");
            UpdateChart(mseChart1, "MSE");


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var mse128 = new DataTable();
            mse128.Columns.Add("Container Name", typeof(string));
            mse128.Columns.Add("Encryption MSE", typeof(decimal));
            mse128.Columns.Add("Size", typeof(int));

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 128)
                .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse128.Rows.Add(group.Key, Math.Round(max, 2),
                    groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }

            mseChart2.Series.Clear();
            mseChart2.DataBindCrossTable(mse128.Rows, "Container Name", "Size", "Encryption MSE", "");
            mseChart2.Palette = ChartColorPalette.SeaGreen;
            mseChart2.Titles.Clear();
            mseChart2.Titles.Add("128x128 watermark results");
            UpdateChart(mseChart2, "MSE");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var mse256 = new DataTable();
            mse256.Columns.Add("Container Name", typeof(string));
            mse256.Columns.Add("Encryption MSE", typeof(decimal));
            mse256.Columns.Add("Size", typeof(int));
            mseChart3.Titles.Clear();
            mseChart3.Titles.Add("256x256 watermark results");
            mseChart3.Series.Clear();


            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 256)
                                                                 .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.EncryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse256.Rows.Add(group.Key, Math.Round(max), groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }
            mseChart3.DataBindCrossTable(mse256.Rows, "Container Name", "Size", "Encryption MSE", "");
            UpdateChart(mseChart3, "MSE");

        }

        private void SetPSNRChartsDecryption(List<WatermarkingResults> rows)
        {
            var dt64 = new DataTable();
            dt64.Columns.Add("Container Name", typeof(string));
            dt64.Columns.Add("Decryption PSNR", typeof(decimal));
            dt64.Columns.Add("Size", typeof(int));
            decPsnrChart1.Titles.Clear();

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 64).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt64.Rows.Add(group.Key, Math.Round(max, 2), groupBySize.Key == 128 ? 0 : groupBySize.Key == 256 ? 1 : groupBySize.Key == 512 ? 2 : 3);
                }
            }

            decPsnrChart1.Series.Clear();
            decPsnrChart1.DataBindCrossTable(dt64.Rows, "Container Name", "Size", "Decryption PSNR", "");
            decPsnrChart1.Palette = ChartColorPalette.Berry;
            decPsnrChart1.Titles.Add("64x64 watermark results");
            UpdateChart(decPsnrChart1, "PSNR");


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var dt128 = new DataTable();
            dt128.Columns.Add("Container Name", typeof(string));
            dt128.Columns.Add("Decryption PSNR", typeof(decimal));
            dt128.Columns.Add("Size", typeof(int));
            decPsnrChart2.Series.Clear();

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 128)
                .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt128.Rows.Add(group.Key, Math.Round(max, 2),
                    groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }

            decPsnrChart2.DataBindCrossTable(dt128.Rows, "Container Name", "Size", "Decryption PSNR", "");
            decPsnrChart2.Palette = ChartColorPalette.SeaGreen;
            decPsnrChart2.Titles.Clear();
            decPsnrChart2.Titles.Add("128x128 watermark results");

            UpdateChart(decPsnrChart2, "PSNR");


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var dt256 = new DataTable();
            dt256.Columns.Add("Container Name", typeof(string));
            dt256.Columns.Add("Decryption PSNR", typeof(decimal));
            dt256.Columns.Add("Size", typeof(int));
            decPsnrChart3.Titles.Clear();
            decPsnrChart3.Titles.Add("256x256 watermark results");
            decPsnrChart3.Series.Clear();


            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 256)
                                                                 .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionPsnr).Concat(new[] { 0.0 }).Max();
                    dt256.Rows.Add(group.Key, Math.Round(max), groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }
            decPsnrChart3.DataBindCrossTable(dt256.Rows, "Container Name", "Size", "Decryption PSNR", "");

            UpdateChart(decPsnrChart3, "PSNR");

        }

        private void SetMseChartsDecryption(List<WatermarkingResults> rows)
        {
            var mse64 = new DataTable();
            mse64.Columns.Add("Container Name", typeof(string));
            mse64.Columns.Add("Decryption MSE", typeof(decimal));
            mse64.Columns.Add("Size", typeof(int));

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 64).GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse64.Rows.Add(group.Key, Math.Round(max, 2),
                           groupBySize.Key == 128 ? 0 : groupBySize.Key == 256 ? 1 : groupBySize.Key == 512 ? 2 : 3);
                }
            }

            decMseChart1.Series.Clear();
            decMseChart1.DataBindCrossTable(mse64.Rows, "Container Name", "Size", "Decryption MSE", "");
            decMseChart1.Palette = ChartColorPalette.Berry;
            decMseChart1.Titles.Clear();
            decMseChart1.Titles.Add("64x64 watermark results");
            UpdateChart(decMseChart1, "MSE");


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var mse128 = new DataTable();
            mse128.Columns.Add("Container Name", typeof(string));
            mse128.Columns.Add("Decryption MSE", typeof(decimal));
            mse128.Columns.Add("Size", typeof(int));

            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 128)
                .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse128.Rows.Add(group.Key, Math.Round(max, 2),
                    groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }

            decMseChart2.Series.Clear();
            decMseChart2.DataBindCrossTable(mse128.Rows, "Container Name", "Size", "Decryption MSE", "");
            decMseChart2.Palette = ChartColorPalette.SeaGreen;
            decMseChart2.Titles.Clear();
            decMseChart2.Titles.Add("128x128 watermark results");
            UpdateChart(decMseChart2, "MSE");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var mse256 = new DataTable();
            mse256.Columns.Add("Container Name", typeof(string));
            mse256.Columns.Add("Decryption MSE", typeof(decimal));
            mse256.Columns.Add("Size", typeof(int));
            decMseChart3.Titles.Clear();
            decMseChart3.Series.Clear();
            decMseChart3.Titles.Add("256x256 watermark results");


            foreach (var group in rows.Where(x => x.WatermarkHeight.GetValueOrDefault() == 256)
                                                                 .GroupBy(x => x.ContainerFileName))
            {
                foreach (var groupBySize in group.GroupBy(x => x.ContainerWidth))
                {
                    var max = groupBySize.Select(row => row.DecryptionMse.GetValueOrDefault()).Concat(new[] { 0.0 }).Max();
                    mse256.Rows.Add(group.Key, Math.Round(max), groupBySize.Key == 256 ? 0 : groupBySize.Key == 512 ? 1 : groupBySize.Key == 1024 ? 2 : 3);
                }
            }
            decMseChart3.DataBindCrossTable(mse256.Rows, "Container Name", "Size", "Decryption MSE", "");
            UpdateChart(decMseChart3, "MSE");

        }

        private void UpdateChart(Chart chart, string text, string oxisLabel="")
        {

            chart.Palette = ChartColorPalette.SeaGreen;

            var series = chart.Series[0]; //series object
            var chartArea = chart.ChartAreas[series.ChartArea];

            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = -0.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 0.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 1.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 2.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 3.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 4.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 5.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 6.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 7.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.StripLines.Add(new StripLine
            {
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 3,
                BorderColor = Color.BlueViolet,
                Interval = 0, // to show only one vertical line
                IntervalOffset = 8.5, // for showing Vertical line between 2 series 
                IntervalType = DateTimeIntervalType.Years // for me years
            });
            chartArea.AxisX.Title = string.IsNullOrWhiteSpace(oxisLabel) ? "Container size (squad)" : oxisLabel;
            chartArea.AxisY.Title = text;
        }

        private void size64Chart_Click(object sender, EventArgs e)
        {

        }

        private void chart5_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }
    }
}
