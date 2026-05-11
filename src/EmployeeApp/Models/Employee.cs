using System.ComponentModel.DataAnnotations;

namespace EmployeeApp.Models;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "氏名は必須です")]
    public string Name { get; set; } = string.Empty;

    public int? DeptId { get; set; }

    [Range(0, 100_000_000, ErrorMessage = "給与は 0 以上 1 億以下で入力してください")]
    public decimal? Salary { get; set; }

    public DateOnly? HireDate { get; set; }
    public int? ManagerId { get; set; }
    public string? DeptName { get; set; }
}
