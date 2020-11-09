using Algorithm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        public string ContainerFileName { get; set; }
        public string WatermarkFileName { get; set; }

        public Form1()
        {
            InitializeComponent();
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
            ContainerPictureBox.Image = Image.FromFile(openFileDialog1.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            WatermarkFileName = openFileDialog2.FileName;
            WatermarkPictureBox.Image = Image.FromFile(openFileDialog2.FileName);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var originalFileName = $"{Path.GetFileNameWithoutExtension(ContainerFileName)}_{Path.GetFileNameWithoutExtension(WatermarkFileName)}";
            var container = new Bitmap(ContainerFileName ?? string.Empty);
            var watermark = new Bitmap(WatermarkFileName ?? string.Empty);
            var encryptionResult = await Executor.HandleEncryption(container, watermark, originalFileName);

            EditedContainerPictureBox.Image = encryptionResult.ContainerWithWatermark;
            var decryptionResult = await Executor.Decrypt(originalFileName, watermark);
            EditedWatermarkPictureBox.Image = decryptionResult.ExtractedWatermark;

            SetLabelsVisibility(encryptionResult, decryptionResult);
        }


        private void SetLabelsVisibility(ProcessingResult encryptionResult, ProcessingResult decryptionResult)
        {

            label10.Text = decryptionResult.Time.TotalMilliseconds.ToString();
            label11.Text = encryptionResult.Time.TotalMilliseconds.ToString();
            label12.Text = Math.Round(decryptionResult.Psnr,2).ToString();
            label13.Text = Math.Round(encryptionResult.Psnr, 2).ToString();
            label14.Text = $@"{encryptionResult.WatermarkHeight}x{encryptionResult.WatermarkWidth}";
            label15.Text = $@"{encryptionResult.ContainerHeight}x{encryptionResult.ContainerWidth}";
            label16.Text = encryptionResult.AverageBlueColor.ToString();
            label17.Text = encryptionResult.AverageGreenColor.ToString();
            label18.Text = encryptionResult.AverageRedColor.ToString();

            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            label11.Visible = true;
            label12.Visible = true;
            label13.Visible = true;
            label14.Visible = true;
            label15.Visible = true;
            label16.Visible = true;
            label17.Visible = true;
            label18.Visible = true;
        }
    }
}
