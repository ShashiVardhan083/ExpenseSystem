using System.Diagnostics;
using ExpenseSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseSystem.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> Logger;

    public HomeController(ILogger<HomeController> logger)
    {
        Logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error/{statusCode?}")]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500
        };

        return View(model);
    }
    public IActionResult Crash()
    {
        throw new Exception("Test exception");
    }

}