using Microsoft.AspNetCore.Mvc;

namespace ExpenseSystem.MockPaymentAPI.Controllers;

// ─── DTOs for the Mock Payment API ───────────────────────────────
// These are LOCAL to the mock API — no shared project dependency.
// In real life, this would be documented in a contract/spec.

public class PaymentRequest
{
    public Guid ExpenseId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// MOCK PAYMENT CONTROLLER
///
/// Simulates an external payment processor.
/// The ExpenseSystem.Infrastructure.PaymentGateway calls this
/// via HttpClient when processing an approved expense.
///
/// Demonstrates:
///  - How HttpClient makes POST calls between services
///  - Service-to-service communication pattern
///  - What the "other side" of HttpClient looks like
///
/// SIMULATION LOGIC:
///   Amount > 10,000 → Rejected (over limit)
///   Any other valid amount → Approved with reference
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Process a payment request from the Expense System.
    /// Called by ExpenseSystem.Infrastructure.PaymentGateway via HttpClient.
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Process([FromBody] PaymentRequest request)
    {
        _logger.LogInformation(
            "Payment request received: ExpenseId={ExpenseId}, Employee={Employee}, Amount={Amount}",
            request.ExpenseId, request.EmployeeName, request.Amount);

        // Simulate: reject payments over $10,000
        if (request.Amount > 10_000)
        {
            return UnprocessableEntity(new PaymentResponse
            {
                Success = false,
                Message = $"Amount ${request.Amount:F2} exceeds single transaction limit of $10,000."
            });
        }

        // Simulate: reject negative or zero amounts
        if (request.Amount <= 0)
        {
            return BadRequest(new PaymentResponse
            {
                Success = false,
                Message = "Amount must be greater than zero."
            });
        }

        // Generate a fake payment reference
        var reference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        _logger.LogInformation("Payment approved with reference: {Reference}", reference);

        return Ok(new PaymentResponse
        {
            Success = true,
            Reference = reference,
            Message = $"Payment of ${request.Amount:F2} to {request.EmployeeName} processed successfully."
        });
    }
}