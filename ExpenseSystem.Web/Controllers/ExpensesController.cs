using ExpenseSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseSystem.Application.DTOs;

namespace ExpenseSystem.Web.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly IExpenseApiClient ApiClient;
    private readonly ILogger<ExpensesController> Logger;

    public ExpensesController(
        IExpenseApiClient apiClient,
        ILogger<ExpensesController> logger)
    {
        ApiClient = apiClient;
        Logger = logger;
    }

    // =========================================================
    // EMPLOYEE SECTION
    // =========================================================

    // GET: /Expenses
    // Employee sees only their expenses
    [Authorize(Roles = "Employee")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.Time = DateTime.Now.ToString("HH:mm:ss");

            var expenses = await ApiClient.GetAllExpensesAsync();
            return View(expenses);
        }
        catch (ApplicationException ex)
        {
            Logger.LogError(ex, "Failed to load employee expenses");
            TempData["ErrorMessage"] = ex.Message;
            return View(Enumerable.Empty<ExpenseResponseDto>());
        }
    }

    // GET: /Expenses/Create
    [Authorize(Roles = "Employee")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Expenses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Create(CreateExpenseDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var created = await ApiClient.CreateExpenseAsync(dto);

        TempData["SuccessMessage"] = "Expense submitted successfully!";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }


    // =========================================================
    // SHARED SECTION (Employee can view their own,
    // Admin can review all)
    // =========================================================

    // GET: /Expenses/Details/{id}
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var expense = await ApiClient.GetExpenseByIdAsync(id);

            if (expense == null)
            {
                TempData["ErrorMessage"] = "Expense not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(expense);
        }
        catch (ApplicationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // =========================================================
    // ADMIN SECTION
    // =========================================================

    // POST: /Expenses/Approve/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await ApiClient.ApproveExpenseAsync(id);
            TempData["SuccessMessage"] = "Expense approved successfully!";
        }
        catch (ApplicationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Expenses/Reject/{id}
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            var expense = await ApiClient.GetExpenseByIdAsync(id);

            if (expense == null)
            {
                TempData["ErrorMessage"] = "Expense not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Expense = expense;
            return View(new RejectExpenseDto());
        }
        catch (ApplicationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Expenses/Reject/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, RejectExpenseDto dto)
    {
        if (!ModelState.IsValid)
        {
            var expense = await ApiClient.GetExpenseByIdAsync(id);
            ViewBag.Expense = expense;
            return View(dto);
        }

        try
        {
            await ApiClient.RejectExpenseAsync(id, dto);
            TempData["SuccessMessage"] = "Expense rejected successfully.";
        }
        catch (ApplicationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Expenses/ProcessPayment/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> ProcessPayment(Guid id)
    {
        try
        {
            var result = await ApiClient.ProcessPaymentAsync(id);
            TempData["SuccessMessage"] =
                $"Payment processed successfully! Reference: {result.PaymentReference}";
        }
        catch (ApplicationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
