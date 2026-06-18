namespace EmployeeApp.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? DeptId { get; set; }
    public decimal? Salary { get; set; }
    public DateOnly? HireDate { get; set; }
    public int? ManagerId { get; set; }
}
