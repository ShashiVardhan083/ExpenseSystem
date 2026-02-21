using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseSystem.Web.Services;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IExpenseApiClient _apiClient;

    public AdminController(IExpenseApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> ManageExpenses()
    {
        var expenses = await _apiClient.GetAllExpensesAsync();

        ViewBag.Time = DateTime.Now.ToLongTimeString();

        return View(expenses);
    }
}
