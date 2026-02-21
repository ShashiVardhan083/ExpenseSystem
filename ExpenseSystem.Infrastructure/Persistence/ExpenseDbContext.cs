using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSystem.Infrastructure.Persistence;

public class ExpenseDbContext : DbContext
{
    public DbSet<ExpenseClaim> ExpenseClaims => Set<ExpenseClaim>();
    public DbSet<User> Users => Set<User>();

    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExpenseClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.SubmittedByUserId).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Role).HasConversion<string>().IsRequired();
            entity.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(u => u.CreatedDate).IsRequired();
            entity.Property(u => u.LastLoginDate).IsRequired(false);
        });

        // Seed admin and employee accounts
        // Passwords: Admin123! and Employee123!
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "admin@expense.com",
                FullName = "System Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Admin123!
                Role = UserRole.Admin,
                CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                LastLoginDate = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = "employee@expense.com",
                FullName = "John",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"), // Employee123!
                Role = UserRole.Employee,
                CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                LastLoginDate = (DateTime?)null
            }
        );
    }
}