namespace ITAssetManager.Models
{
    public class VwEmployee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
