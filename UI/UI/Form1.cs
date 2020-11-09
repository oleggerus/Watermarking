using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Algorithm;

namespace UI
{
    public partial class Form1 : Form
    {
        public string ContainerFileName { get; set; }
        public string WatermarkFileName { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ContainerPictureBox_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            ContainerFileName = openFileDialog1.FileName;
            ContainerPictureBox.Image = Image.FromFile(openFileDialog1.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            ContainerPictureBox.Image = Image.FromFile(openFileDialog1.FileName);
            ContainerFileName = openFileDialog1.FileName;

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            WatermarkFileName = openFileDialog2.FileName;
            WatermarkPictureBox.Image = Image.FromFile(openFileDialog2.FileName);
        }

        private async void RunExecutorButton_Click(object sender, EventArgs e)
        {
            var originalFileName = $"{Path.GetFileNameWithoutExtension(ContainerFileName)}_{Path.GetFileNameWithoutExtension(WatermarkFileName)}";
            var container = new Bitmap(ContainerFileName);
            var watermark = new Bitmap(WatermarkFileName);
            var result = await Executor.HandleEncryption(container, watermark, originalFileName);
            
            EditedContainerPictureBox.Image = result.ContainerWithWatermark;
            var decryptionResult = await Executor.Decrypt(originalFileName, watermark);
            EditedWatermarkPictureBox.Image = decryptionResult.ExtractedWatermark;
        }
    }
}
