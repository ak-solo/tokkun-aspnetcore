using Dapper;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EmployeeApp.Controllers;

public class EmployeeController : Controller
{
    private readonly IDbConnection _db;

    public EmployeeController(IDbConnection db)
    {
        _db = db;
    }

    public IActionResult Index(string? keyword, int? deptId, string? sortBy, string? sortDir)
    {
        var validColumns = new[] { "name", "salary", "hire_date" };
        var column    = validColumns.Contains(sortBy) ? sortBy : "hire_date";
        var direction = sortDir == "asc" ? "ASC" : "DESC";

        var sql = @"
            SELECT id, name, dept_id, salary, hire_date, manager_id
            FROM employees
            WHERE 1=1
        ";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql += " AND name LIKE @keyword";
            parameters.Add("keyword", $"%{keyword}%");
        }

        if (deptId.HasValue)
        {
            sql += " AND dept_id = @deptId";
            parameters.Add("deptId", deptId.Value);
        }

        sql += $" ORDER BY {column} {direction}";

        ViewData["Keyword"] = keyword;
        ViewData["DeptId"]  = deptId;
        ViewData["SortBy"]  = column;
        ViewData["SortDir"] = direction.ToLower();

        var employees = _db.Query<Employee>(sql, parameters);
        return View(employees);
    }

    public IActionResult Details(int id)
    {
        var employee = _db.QueryFirstOrDefault<Employee>(
            @"SELECT e.*, d.name AS dept_name
              FROM employees e
              LEFT JOIN departments d ON e.dept_id = d.id
              WHERE e.id = @id",
            new { id });
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Employee employee)
    {
        var id = _db.QuerySingle<int>(
            @"INSERT INTO employees (name, dept_id, salary, hire_date)
              VALUES (@Name, @DeptId, @Salary, @HireDate)
              RETURNING id",
            employee);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var employee = _db.QueryFirstOrDefault<Employee>(
            "SELECT * FROM employees WHERE id = @id",
            new { id });
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        _db.Execute(
            @"UPDATE employees
              SET name = @Name, dept_id = @DeptId, salary = @Salary, hire_date = @HireDate
              WHERE id = @Id",
            employee);
        return RedirectToAction(nameof(Details), new { employee.Id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var employee = _db.QueryFirstOrDefault<Employee>(
            "SELECT * FROM employees WHERE id = @id",
            new { id });
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _db.Execute("DELETE FROM employees WHERE id = @id", new { id });
        return RedirectToAction(nameof(Index));
    }
}
