//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class WatermarkingResults
    {
        public int Id { get; set; }
        public string ContainerFileName { get; set; }
        public string KeyFileName { get; set; }
        public System.TimeSpan EncryptionTime { get; set; }
        public System.TimeSpan DecryptionTime { get; set; }
        public double EncryptionPsnr { get; set; }
        public double DecryptionPsnr { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> Brightness { get; set; }
        public Nullable<int> Contrast { get; set; }
        public Nullable<int> AverageRedColor { get; set; }
        public Nullable<int> AverageGreenColor { get; set; }
        public Nullable<int> AverageBlueColor { get; set; }
        public Nullable<int> Mode { get; set; }
        public Nullable<int> ContainerWidth { get; set; }
        public Nullable<int> ContainerHeight { get; set; }
        public Nullable<int> WatermarkWidth { get; set; }
        public Nullable<int> WatermarkHeight { get; set; }
        public Nullable<int> AverageRedColorWatermark { get; set; }
        public Nullable<int> AverageGreenColorWatermark { get; set; }
        public Nullable<int> AverageBlueColorWatermark { get; set; }
    }
}
