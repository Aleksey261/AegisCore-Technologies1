using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers;

public class AuthController : Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<IActionResult> LoginPost(string username, string password)
    {
        if (username == "admin" && password == "admin123")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, "Administrator")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Dashboard", "Home");
        }
        ViewBag.Error = "Неверный логин или пароль";
        return View("Login");
    }

    [Authorize]
    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
