using AutoMapper;
using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Domain;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
namespace ExpenseSystem.Application.Services;
public class AuthService
{
    private readonly IUserRepository UserRepository;
    private readonly ITokenService TokenService;
    private readonly IMapper Mapper;
    private readonly ILogger<AuthService> Logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        UserRepository = userRepository;
        TokenService = tokenService;
        Mapper = mapper;
        Logger = logger;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        Logger.LogInformation("Registering new user: {Email}", dto.Email);

        if (await UserRepository.EmailExistsAsync(dto.Email))
        {
            Logger.LogWarning("Registration failed - email already exists: {Email}", dto.Email);
            throw new DomainException("An account with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Email, dto.FullName, passwordHash, UserRole.Employee);

        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        Logger.LogInformation("User registered successfully: {Email}, ID: {UserId}", dto.Email, user.Id);

        return Mapper.Map<UserResponseDto>(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        Logger.LogInformation("Login attempt for: {Email}", dto.Email);

        var user = await UserRepository.GetByEmailAsync(dto.Email);

        if (user == null)
        {
            Logger.LogWarning("Login failed - user not found: {Email}", dto.Email);
            throw new DomainException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            Logger.LogWarning("Login failed - account inactive: {Email}", dto.Email);
            throw new DomainException("This account has been deactivated.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            Logger.LogWarning("Login failed - invalid password for: {Email}", dto.Email);
            throw new DomainException("Invalid email or password.");
        }

        user.RecordLogin();
        await UserRepository.SaveChangesAsync();

        var token = TokenService.GenerateToken(user);

        Logger.LogInformation("Login successful for: {Email}", dto.Email);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = Mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await UserRepository.GetByIdAsync(userId);
        return user == null ? null : Mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await UserRepository.GetAllAsync();
        return Mapper.Map<IEnumerable<UserResponseDto>>(users);
    }
}