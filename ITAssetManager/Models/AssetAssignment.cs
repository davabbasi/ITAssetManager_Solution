namespace ITAssetManager.Models;

public class AssetAssignment
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    // از کجا
    public int? FromEmployeeId { get; set; }
    public Employee? FromEmployee { get; set; }
    public int? FromDepartmentId { get; set; }
    public Department? FromDepartment { get; set; }
    public string? FromLocation { get; set; }

    // به کجا
    public int? ToEmployeeId { get; set; }
    public Employee? ToEmployee { get; set; }
    public int? ToDepartmentId { get; set; }
    public Department? ToDepartment { get; set; }
    public string? ToLocation { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.Now;
    public string? Reason { get; set; }
    public string? AssignedBy { get; set; } // نام کاربر ثبت‌کننده
    public string? Notes { get; set; }
}
