#region Imports

using Microsoft.AspNetCore.Mvc;

#endregion

namespace Portfolio.Controllers;

public class ErrorController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ErrorController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    //GET
    [HttpGet]
    public IActionResult AccessDeniedError()
    {
        return View();
    }

    // GET
    [HttpGet]
    [Route("Error/{statusCode}")]
    public IActionResult NotFoundError(int statusCode)
    {
        if (statusCode == 404)
            return View("404");

        return View("404");
    }
}