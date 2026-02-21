using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseService ExpenseService;
    private readonly ILogger<ExpensesController> Logger;

    public ExpensesController(ExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        ExpenseService = expenseService;
        Logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user ID in token.");

        return userId;
    }

    private bool IsAdmin()
        => User.IsInRole("Admin");

    // GET /api/expenses
    // Admin: all expenses | Employee: only their own
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();

        if (IsAdmin())
        {
            Logger.LogInformation("Admin {UserId} fetching all expenses", userId);
            var all = await ExpenseService.GetAllExpensesAsync();
            return Ok(all);
        }

        Logger.LogInformation("Employee {UserId} fetching their expenses", userId);
        var mine = await ExpenseService.GetExpensesByUserAsync(userId);
        return Ok(mine);
    }

    // GET /api/expenses/{id}
    // Admin: any expense | Employee: only their own
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var expense = await ExpenseService.GetExpenseAsync(id);

        if (expense is null)
            return NotFound(new { message = $"Expense '{id}' not found." });

        var userId = GetCurrentUserId();

        // Employees can only view their own expenses
        if (!IsAdmin() && expense.SubmittedByUserId != userId)
            return Forbid();

        return Ok(expense);
    }

    // POST /api/expenses
    // Any authenticated user can submit an expense
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var userId = GetCurrentUserId();
        Logger.LogInformation("User {UserId} creating expense for {Employee}", userId, dto.EmployeeName);

        var created = await ExpenseService.CreateExpenseAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ── PUT /api/expenses/{id}/approve ────────────────────────────────
    // Admin only
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = GetCurrentUserId();
        Logger.LogInformation("Admin {AdminId} approving expense {ExpenseId}", adminId, id);
        var updated = await ExpenseService.ApproveExpenseAsync(id);
        return Ok(updated);
    }

    // ── PUT /api/expenses/{id}/reject ─────────────────────────────────
    // Admin only
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseDto dto)
    {
        var adminId = GetCurrentUserId();
        Logger.LogWarning("Admin {AdminId} rejecting expense {ExpenseId}, Reason: {Reason}", adminId, id, dto.Reason);
        var updated = await ExpenseService.RejectExpenseAsync(id, dto);
        return Ok(updated);
    }

    // ── POST /api/expenses/{id}/process-payment ───────────────────────
    // Admin only
    [HttpPost("{id:guid}/process-payment")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ProcessPayment(Guid id)
    {
        var userId = GetCurrentUserId();

        Logger.LogInformation(
            "Employee {UserId} attempting payment for expense {ExpenseId}",
            userId, id);

        var updated = await ExpenseService.ProcessPaymentAsync(id, userId);

        return Ok(updated);
    }


}