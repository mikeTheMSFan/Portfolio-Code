#region Imports

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Enums;
using Portfolio.Extensions;
using Portfolio.Models;
using Portfolio.Models.ViewModels;
using Portfolio.Services.Interfaces;
using SmartBreadcrumbs.Attributes;
using X.PagedList;

#endregion

namespace Portfolio.Controllers;

public class HomeController : Controller
{
    private readonly IMWSBlogService _blogService;
    private readonly IMWSEmailService _emailService;
    private readonly IMWSImageService _imageService;
    private readonly IMWSProjectService _projectService;

    public HomeController(IMWSEmailService emailService,
        IMWSProjectService projectService,
        IMWSBlogService blogService,
        IMWSImageService imageService)
    {
        _emailService = emailService;
        _projectService = projectService;
        _blogService = blogService;
        _imageService = imageService;
    }

    [TempData] public string StatusMessage { get; set; } = default!;

    #region Contact get action

    [HttpGet]
    public IActionResult Contact()
    {
        return NotFound();
    }

    #endregion

    #region Contact post action

    [ValidateReCaptcha]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact([FromForm] FrontPageViewModel model)
    {
        if (ModelState.ContainsKey("Recaptcha") &&
            ModelState["Recaptcha"]!.ValidationState == ModelValidationState.Invalid)
        {
            StatusMessage = "Error verifying reCAPTCHA, please try again.";
            return RedirectToAction("Index");
        }

        try
        {
            model.ContactEmail.Body =
                $"<p>New Message From User ✅✅✅</p><p>Email:{model.ContactEmail.Email}</p>Body:</p><p>{model.ContactEmail.Body}</p>";

            //send contact email.
            await _emailService.SendContactEmailAsync(model.ContactEmail.Email, model.ContactEmail.Name,
                model.ContactEmail.Subject, model.ContactEmail.Body);
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error contact email****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("****************************************************");
            throw;
        }

        StatusMessage = "Your message has been received, I will get back to you soon.";
        return RedirectToAction("Index");
    }

    #endregion

    #region Error action

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #endregion

    #region Index get action

    [DefaultBreadcrumb("Home")]
    public async Task<IActionResult> Index()
    {
        var blog = await _blogService.GetFirstBlog();

        var posts = await blog.Posts.OrderByDescending(p => p.Created)
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .Take(3)
            .ToListAsync();

        foreach (var post in posts) post.Base64PostPicture = _imageService.DecodeImage(post.Image!, post.ImageType!);

        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        var model = new FrontPageViewModel
        {
            Projects = await _projectService.GetAllProjectsAsync(),
            Posts = posts
        };

        //var pageSize = 4;
        //var pageCount = (double)((await _projectService.GetAllProjectsAsync()).Count / Convert.ToDecimal(pageSize));
        //pageCount = (int)Math.Ceiling(pageCount);
        //ViewBag.PageCount = pageCount;

        var projectSelectListItems = new List<SelectListItem>().CreateProjectSelectListItems();
        var projectCategories = projectSelectListItems.Select(pc => pc.Text).ToList();
        ViewBag.ProjectCategories = projectCategories;

        return View(model);
    }

    #endregion

    #region Privacy

    public IActionResult Privacy()
    {
        return View();
    }

    #endregion
}