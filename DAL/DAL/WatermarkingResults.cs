using System;
using System.Collections.Generic;

namespace DAL.DAL
{
    public partial class WatermarkingResults
    {
        public int Id { get; set; }
        public string ContainerFileName { get; set; }
        public string KeyFileName { get; set; }
        public TimeSpan EncryptionTime { get; set; }
        public TimeSpan DecryptionTime { get; set; }
        public double EncryptionPsnr { get; set; }
        public double DecryptionPsnr { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? Brightness { get; set; }
        public int? Contrast { get; set; }
    }
}
