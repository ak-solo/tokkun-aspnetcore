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

    public IActionResult Index()
    {
        var employees = _db.Query<Employee>(
            "SELECT id, name, dept_id, salary, hire_date, manager_id FROM employees ORDER BY hire_date DESC");
        return View(employees);
    }

    public IActionResult Details(int id)
    {
        var employee = _db.QueryFirstOrDefault<Employee>(
            "SELECT * FROM employees WHERE id = @id",
            new { id });
        if (employee == null) return NotFound();
        return View(employee);
    }
}
