namespace ITAssetManager.Models
{
    public class AssemblyComponent
    {
        public int Id { get; set; }

        public int PcAssetId { get; set; }       // PC ای که این قطعه توشه
        public Asset? PcAsset { get; set; }

        public int ComponentAssetId { get; set; } // قطعه (مادربرد، رم، CPU و...)
        public Asset? ComponentAsset { get; set; }

        public DateTime InstalledAt { get; set; } = DateTime.Now;
        public DateTime? RemovedAt { get; set; }   // وقتی از PC جدا میشه پر میشه
        public string? InstalledBy { get; set; }
        public string? RemovedBy { get; set; }
        public string? Notes { get; set; }

        public bool IsActive => RemovedAt == null;  // هنوز نصبه یا نه
    }
}
