using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers;

public class HomeController : Controller
{
    private readonly JsonDataStore _store;
    public HomeController(JsonDataStore store) => _store = store;

    [AllowAnonymous]
    [HttpGet("/")]
    [HttpGet("/index")]
    [HttpGet("/home")]
    public IActionResult Index() => View("Index");

    [Authorize]
    [HttpGet("/dashboard")]
    public IActionResult Dashboard() => View(_store.GetStats());

    [AllowAnonymous]
    [HttpGet("/error")]
    public IActionResult Error() => View("Error");
}
