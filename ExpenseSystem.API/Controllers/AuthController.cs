using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService AuthService;
    private readonly ILogger<AuthController> Logger;
    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        AuthService = authService;
        Logger = logger;
    }
    /// Register a new user account (returns Employee role)
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        Logger.LogInformation("Registration request for: {Email}", dto.Email);
        var user = await AuthService.RegisterAsync(dto);
        Logger.LogInformation("User registered: {UserId}", user.Id);
        return CreatedAtAction(nameof(GetCurrentUser), null, user);
    }
    /// <summary>Login and receive a JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        Logger.LogInformation("Login request for: {Email}", dto.Email);
        var response = await AuthService.LoginAsync(dto);
        Logger.LogInformation("Login successful for: {Email}", dto.Email);
        return Ok(response);
    }

    /// <summary>Get current authenticated user info</summary>
    [HttpGet("CurrentUser")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            Logger.LogWarning("Invalid user ID in JWT token");
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await AuthService.GetUserByIdAsync(userId);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>Get all users — Admin only</summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await AuthService.GetAllUsersAsync();
        return Ok(users);
    }
}