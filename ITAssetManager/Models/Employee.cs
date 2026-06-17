namespace ITAssetManager.Models;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Position { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
    public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
}
