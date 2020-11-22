﻿using Algorithm;
using DAL;
using DAL.Services;
using SVD;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            WatermarkFileName = openFileDialog2.FileName;
            if (!string.IsNullOrWhiteSpace(WatermarkFileName) && WatermarkFileName != "openFileDialog2")
            {
                WatermarkPictureBox.Image = Image.FromFile(openFileDialog2.FileName);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
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
        }


        private void SetLabelsVisibility(ProcessingResult encryptionResult, ProcessingResult decryptionResult)
        {

          
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

            var extractedWatermark = Helpers.CalculateColors(WatermarkedContainer);
            label25.Text = extractedWatermark.Item3.ToString();
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
        }

        private async Task PopulateGridWithFullData()
        {
            if (displayOriginalRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = await DalService.GetAllResultByMode(new List<int> { 0, 3, 4, 5 });

                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName, item.KeyFileName, "-", "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayContrastContainerRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode(1))
                    .Where(x => x.Brightness.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName, item.KeyFileName, item.Contrast, "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayBrightnessContainerRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode(1))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName, item.KeyFileName, "-", item.Brightness,
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
                GridViewTab3.Refresh();
            }
            else if (displayContrastWatermarkRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode(2))
                    .Where(x => x.Brightness.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName, item.KeyFileName, item.Contrast, "-",
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
                        item.EncryptionTime.TotalMilliseconds, item.DecryptionTime.TotalMilliseconds);
                }
            }
            else if (displayBrightnessWatermarkRadionBtn.Checked)
            {
                GridViewTab3.Rows.Clear();
                var toDisplay = (await DalService.GetAllResultByMode(2))
                    .Where(x => x.Contrast.GetValueOrDefault() == 0).ToList();
                foreach (var item in toDisplay)
                {
                    GridViewTab3.Rows.Add(item.ContainerFileName, item.KeyFileName, "-", item.Brightness,
                        $"{item.ContainerHeight}x{item.ContainerWidth}",
                        $"{item.WatermarkHeight}x{item.WatermarkHeight}",
                        $"{item.AverageRedColor} - {item.AverageGreenColor} - {item.AverageBlueColor}",
                        $"{item.AverageRedColorWatermark} - {item.AverageGreenColorWatermark} - {item.AverageBlueColorWatermark}",
                        Math.Round(item.EncryptionPsnr, 2), Math.Round(item.DecryptionPsnr, 2),
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
            OriginalContainer = Helpers.ResizeImage(WatermarkedContainer, OriginalContainer.Width/2, OriginalContainer.Height/2);
            ContainerPictureBox.Image = OriginalContainer;
        }

        private void ResizeBtn_Click(object sender, EventArgs e)
        {
            WatermarkedContainer = Helpers.ResizeImage(WatermarkedContainer, WatermarkedContainer.Width / 2, WatermarkedContainer.Height / 2);
            EditedContainerPictureBox.Image = WatermarkedContainer;
        }
    }
}
