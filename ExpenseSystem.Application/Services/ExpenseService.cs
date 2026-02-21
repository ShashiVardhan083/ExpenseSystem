using AutoMapper;
using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Domain;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ExpenseSystem.Application.Services;

public class ExpenseService
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository repository,
        IPaymentGateway paymentGateway,
        IMapper mapper,
        ILogger<ExpenseService> logger)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
        _mapper = mapper;
        _logger = logger;
    }

    // Create (Employee only — pass their userId)
    public async Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto, Guid submittedByUserId)
    {
        var expense = ExpenseClaim.Create(
            dto.EmployeeName,
            dto.Description,
            dto.Amount,
            submittedByUserId);

        await _repository.AddAsync(expense);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Expense {ExpenseId} created by user {UserId}", expense.Id, submittedByUserId);

        return _mapper.Map<ExpenseResponseDto>(expense);
    }

    // Get single expense 
    public async Task<ExpenseResponseDto?> GetExpenseAsync(Guid id)
    {
        var expense = await _repository.GetByIdAsync(id);
        return expense is null ? null : _mapper.Map<ExpenseResponseDto>(expense);
    }

    // Get all expenses (Admin)
    public async Task<IEnumerable<ExpenseResponseDto>> GetAllExpensesAsync()
    {
        var expenses = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
    }

    // Get expenses for a specific user (Employee)
    public async Task<IEnumerable<ExpenseResponseDto>> GetExpensesByUserAsync(Guid userId)
    {
        var expenses = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
    }

    // ── Approve (Admin only) ─────────────────────────────────────────
    public async Task<ExpenseResponseDto> ApproveExpenseAsync(Guid id)
    {
        var expense = await GetOrThrowAsync(id);
        expense.Approve();
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Expense {ExpenseId} approved", id);
        return _mapper.Map<ExpenseResponseDto>(expense);
    }

    // Reject (Admin only) 
    public async Task<ExpenseResponseDto> RejectExpenseAsync(Guid id, RejectExpenseDto dto)
    {
        var expense = await GetOrThrowAsync(id);
        expense.Reject(dto.Reason);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Expense {ExpenseId} rejected", id);
        return _mapper.Map<ExpenseResponseDto>(expense);
    }

    //  Process Payment (Employee only)
    public async Task<ExpenseResponseDto> ProcessPaymentAsync(Guid id, Guid userId)
    {
        var expense = await GetOrThrowAsync(id);

        if (expense.SubmittedByUserId != userId)
            throw new UnauthorizedAccessException(
                "You cannot process payment for another user's expense.");

        if (expense.Status != ExpenseStatus.Approved)
            throw new DomainException(
                "Only approved expenses can be paid.");

        if (expense.Status == ExpenseStatus.Paid)
            throw new DomainException(
                "Expense has already been paid.");

        var paymentRequest = _mapper.Map<PaymentRequestDto>(expense);
        var paymentResult = await _paymentGateway.ProcessAsync(paymentRequest);

        if (!paymentResult.Success)
            throw new ApplicationException(
                $"Payment gateway rejected the transaction: {paymentResult.Message}");

        expense.MarkAsPaid(paymentResult.Reference);

        await _repository.SaveChangesAsync();

        return _mapper.Map<ExpenseResponseDto>(expense);
    }



    private async Task<ExpenseClaim> GetOrThrowAsync(Guid id)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense is null)
            throw new KeyNotFoundException($"Expense claim '{id}' was not found.");
        return expense;
    }
}