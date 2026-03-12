using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseSystem.Web.Services;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IExpenseApiClient ApiClient;
    public AdminController(IExpenseApiClient apiClient)
    {
        ApiClient = apiClient;
    }
    public async Task<IActionResult> ManageExpenses()
    {
        var expenses = await ApiClient.GetAllExpensesAsync();

        ViewBag.Time = DateTime.Now.ToLongTimeString();

        return View(expenses);
    }
}
