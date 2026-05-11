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
}
