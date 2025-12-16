using DemoUser.ASP.Clients;
using Microsoft.AspNetCore.Mvc;

namespace DemoUser.ASP.Controllers;

public class AuthController : Controller
{
    private readonly AuthApiClient _auth;

    public AuthController(AuthApiClient auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        var token = await _auth.LoginAsync(username, password);

        if (token is null)
        {
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        HttpContext.Session.SetString("JWT", token);
        return RedirectToAction("Index", "Todo");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string username, string password)
    {
        var success = await _auth.RegisterAsync(username, password);

        if (!success)
        {
            ViewBag.Error = "Registration failed";
            return View();
        }

        return RedirectToAction(nameof(Login));
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWT");
        return RedirectToAction(nameof(Login));
    }
}
