using ExpenseSystem.Web.Models;
using ExpenseSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly IExpenseApiClient ApiClient;
    private readonly ILogger<AccountController> Logger;

    public AccountController(IExpenseApiClient apiClient, ILogger<AccountController> logger)
    {
        ApiClient = apiClient;
        Logger = logger;
    }

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Redirect if already logged in
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Expenses");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken] //CSRF
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var loginResponse = await ApiClient.LoginAsync(model.Email, model.Password);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                new Claim(ClaimTypes.Name, loginResponse.User.FullName),
                new Claim(ClaimTypes.Email, loginResponse.User.Email),
                new Claim(ClaimTypes.Role, loginResponse.User.Role),
                new Claim("JwtToken", loginResponse.Token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = loginResponse.ExpiresAt
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            Logger.LogInformation("User logged in: {Email}", model.Email);
            TempData["SuccessMessage"] = $"Welcome back, {loginResponse.User.FullName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Expenses");
        }
        catch (ApplicationException ex)
        {
            Logger.LogWarning("Login failed for: {Email} — {Error}", model.Email, ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Expenses");

        return View();
    }

    // POST /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken] //CSRF
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await ApiClient.RegisterAsync(model.Email, model.FullName, model.Password, model.ConfirmPassword);

            Logger.LogInformation("User registered: {Email}", model.Email);
            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (ApplicationException ex)
        {
            Logger.LogWarning("Registration failed for: {Email} — {Error}", model.Email, ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    //  POST /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Logger.LogInformation("User logging out: {Name}", User.Identity?.Name);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
}