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
            "SELECT id, name FROM employees ORDER BY id");
        return View(employees);
    }
}
