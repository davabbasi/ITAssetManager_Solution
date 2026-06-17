namespace ITAssetManager.Models;

public enum MaintenanceType
{
    Repair = 1,
    Preventive = 2,
    Upgrade = 3,
    Inspection = 4
}

public class MaintenanceLog
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TechnicianName { get; set; }
    public decimal? Cost { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }
    public bool IsResolved { get; set; } = false;

    public string? Resolution { get; set; }
    public string? Notes { get; set; }
    public string? LoggedBy { get; set; }
}
