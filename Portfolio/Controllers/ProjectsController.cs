#region Imports

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Extensions;
using Portfolio.Models.Content;
using Portfolio.Models.ViewModels;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly IMWSCivilityService _civilityService;
    private readonly IMWSImageService _imageService;
    private readonly IMWSOpenGraphService _openGraphService;
    private readonly IMWSProjectImageService _projectImageService;
    private readonly IMWSProjectService _projectService;

    public ProjectsController(IMWSProjectService projectService,
        IMWSCivilityService civilityService,
        IMWSProjectImageService projectImageService,
        IMWSImageService imageService,
        IMWSOpenGraphService openGraphService)
    {
        _projectService = projectService;
        _civilityService = civilityService;
        _projectImageService = projectImageService;
        _imageService = imageService;
        _openGraphService = openGraphService;
    }

    [TempData] public string StatusMessage { get; set; } = default!;

    #region All author projects get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AllAuthorProjects(int? page)
    {
        var pageNumber = page ?? 1;
        var pageSize = 10;
        var projects = await _projectService.GetAllProjectsAsync();

        return View("AuthorIndex", await projects.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    #region Create project get action

    [Authorize(Roles = "Administrator")]
    public IActionResult Create()
    {
        var model = new ProjectCreateViewModel();
        model.ProjectSelectListItems = model.ProjectSelectListItems!.CreateProjectSelectListItems();
        ViewBag.ProjectMultiSelectList = new MultiSelectList(model.ProjectSelectListItems, "Value", "Text");
        return View(model);
    }

    #endregion

    #region Create project post action

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromForm] ProjectCreateViewModel model)
    {
        var enumSelectListItems = new List<SelectListItem>().CreateProjectSelectListItems();
        var categoryList = model.Project.Categories.First().Split(',').ToList();
        if (ModelState.IsValid)
        {
            //Start validation
            var files = HttpContext.Request.Form.Files;
            foreach (var file in files)
                if (file.IsImage() == false)
                {
                    ModelState.AddModelError("Project.ProjectImages",
                        "There is an error with your images, please check them and try again.");

                    return StatusCode(400, ModelState.AllErrors());
                }

            var error = false;
            if (_civilityService.IsCivil(model.Project.Title).Verdict == false)
            {
                ModelState.AddModelError("Project.Title",
                    "There is an error with your title, please check it and try again.");

                error = true;
            }

            model.Project.Slug = model.Project.Title.Slugify();
            if (await _projectService.IsUniqueAsync(model.Project.Slug) == false)
            {
                ModelState.AddModelError("Project.Title",
                    "Your title is not unique, please change it and try again.");

                error = true;
            }

            if (_civilityService.IsCivil(model.Project.ProjectUrl).Verdict == false)
            {
                ModelState.AddModelError("Project.ProjectUrl",
                    "There is an error with your URL, please check it and try again.");
                error = true;
            }

            if (_civilityService.IsCivil(model.Project.Description).Verdict == false)
            {
                ModelState.AddModelError("Project.Description",
                    "There is an error with your Description, please check it and try again.");
                error = true;
            }

            if (!categoryList.Any() ||
                categoryList.Count > enumSelectListItems.Count)
            {
                ModelState.AddModelError("Project.Categories",
                    "Please make sure you have at least one category.");
                error = true;
            }


            foreach (var projectCategory in categoryList)
                if (_civilityService.IsCivil(projectCategory).Verdict == false)
                {
                    ModelState.AddModelError("Project.Categories",
                        "There is an error with at least one of your category name, please check it and try again.");
                    error = true;
                    break;
                }

            model.Project.Categories = categoryList;


            var masterCategoryList = enumSelectListItems.Select(e => e.Text).ToList();
            var numberOfDifferences = categoryList.Count(cl => masterCategoryList.All(mcl => mcl != cl));
            if (numberOfDifferences > 0) error = true;

            //End validation
            if (error)
            {
                var categorySelectItems = new List<SelectListItem>();
                foreach (var category in categoryList)
                {
                    var selectListItem = new SelectListItem
                    {
                        Text = category,
                        Value = category
                    };
                    categorySelectItems.Add(selectListItem);
                }

                model.ProjectSelectListItems = model.ProjectSelectListItems!.CreateProjectSelectListItems();
                ViewBag.ProjectMultiSelectList =
                    new MultiSelectList(model.ProjectSelectListItems, "Value", "Text", categorySelectItems);

                return StatusCode(400, ModelState.AllErrors());
            }

            model.Project.Created = DateTime.Now.ToUniversalTime();
            await _projectService.AddProjectAsync(model.Project);
            await _projectService.AddProjectCategoriesAsync(model.Project);
            var projectSplashFile = files.OrderBy(f => f.FileName).Last();
            await _openGraphService.AddOpenGraphProjectImageAsync(model.Project, projectSplashFile);
            await _projectImageService.AddProjectImagesAsync(model.Project, files.ToList());
            return StatusCode(201);
        }

        var selectItemList = new List<SelectListItem>();
        foreach (var category in categoryList)
        {
            var selectListItem = new SelectListItem
            {
                Text = category,
                Value = category
            };
            selectItemList.Add(selectListItem);
        }

        model.ProjectSelectListItems = model.ProjectSelectListItems!.CreateProjectSelectListItems();
        ViewBag.ProjectMultiSelectList =
            new MultiSelectList(model.ProjectSelectListItems, "Value", "Text", selectItemList);

        return StatusCode(404, ModelState.AllErrors());
    }

    #endregion

    #region Delete project get action

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        var project = await _projectService.GetProjectAsync(id.Value);
        if (project == new Project()) return NotFound();

        return View(project);
    }

    #endregion

    #region Delete project post action

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (await ProjectExists(id) == false) return NotFound();
        var project = await _projectService.GetProjectAsync(id);
        _openGraphService.DeleteOpenGraphProjectImage(project);
        await _projectService.DeleteProjectAsync(project);

        return RedirectToAction(nameof(Index), "Home");
    }

    #endregion

    #region Project Details Action

    [Route("/Project/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var project = await _projectService.GetProjectBySlugAsync(slug);
        if (project.Id == new Guid()) return NotFound();

        var projects = (await _projectService.GetAllProjectsAsync())
            .OrderBy(pj => pj.Title)
            .ToList();

        var nextProjectIndex = -1;
        var lastProjectIndex = -1;
        var currentProjectIndex = projects.IndexOf(project);

        if (currentProjectIndex != projects.IndexOf(projects.Last())) nextProjectIndex = projects.IndexOf(project) + 1;

        if (currentProjectIndex != projects.IndexOf(projects.First())) lastProjectIndex = projects.IndexOf(project) - 1;

        if (nextProjectIndex != -1) ViewBag.NextProjectIndex = projects[nextProjectIndex].Slug!;

        if (lastProjectIndex != -1) ViewBag.LastProjectIndex = projects[lastProjectIndex].Slug!;

        return View(project);
    }

    #endregion

    #region Edit project get action

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (await ProjectExists(id) == false) return NotFound();
        var model = new ProjectEditViewModel
        {
            Project = await _projectService.GetProjectAsync(id)
        };

        //get base64 version of project images
        foreach (var projectImage in model.Project.ProjectImages!)
        {
            var base64Image = Regex.Replace(_imageService.DecodeImage(projectImage.File, projectImage.FileContentType),
                @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

            var imageDictionary = new Dictionary<string, string>();
            imageDictionary.Add("Image", base64Image);
            imageDictionary.Add("Name", projectImage.Name);
            model.Base64Images.Add(imageDictionary);
        }

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        ViewBag.ImageList = JsonSerializer.Serialize(model.Base64Images, options);
        ViewBag.Images = model.Base64Images;

        model.ProjectSelectListItems = model.ProjectSelectListItems!.CreateProjectSelectListItems();
        var selectedCategories = model.Project.ProjectCategories?.Select(pc => pc.Text).ToList();
        ViewBag.ProjectMultiSelectList =
            new MultiSelectList(model.ProjectSelectListItems, "Value", "Text", selectedCategories);

        return View(model);
    }

    #endregion

    #region Edit project post action

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(Guid id, [FromForm] ProjectEditViewModel model)
    {
        var enumSelectListItems = new List<SelectListItem>().CreateProjectSelectListItems();
        var categoryList = model.Project.Categories.First().Split(',').ToList();
        if (ModelState.IsValid)
        {
            if (await ProjectExists(id) == false) return NotFound();
            var newProject = await _projectService.GetProjectAsync(id);

            newProject.Description = model.Project.Description;
            newProject.Title = model.Project.Title;
            newProject.ProjectUrl = model.Project.ProjectUrl;


            //get base64 version of project images
            foreach (var projectImage in newProject.ProjectImages!)
            {
                var base64Image = _imageService.DecodeImage(projectImage.File, projectImage.FileContentType);

                var imageDictionary = new Dictionary<string, string>();
                imageDictionary.Add("Image", base64Image);
                imageDictionary.Add("Name", projectImage.Name);
                model.Base64Images.Add(imageDictionary);
            }

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            ViewBag.ImageList = JsonSerializer.Serialize(model.Base64Images, options);
            ViewBag.Images = model.Base64Images;

            //Start validation
            var files = HttpContext.Request.Form.Files;
            foreach (var file in files.OrderByDescending(f => f.FileName))
                if (file.IsImage() == false)
                {
                    ModelState.AddModelError("Project.ProjectImages",
                        "There is an error with your images, please check them and try again.");

                    return StatusCode(400, ModelState.AllErrors());
                }

            var error = false;
            if (_civilityService.IsCivil(model.Project.Title).Verdict == false)
            {
                ModelState.AddModelError("Project.Title",
                    "There is an error with your title, please check it and try again.");

                error = true;
            }

            model.Project.Slug = model.Project.Title.Slugify();
            if (model.Project.Slug != newProject.Slug &&
                await _projectService.IsUniqueAsync(model.Project.Slug) == false)
            {
                ModelState.AddModelError("Project.Title",
                    "Your title is not unique, please change it and try again.");

                error = true;
            }

            if (_civilityService.IsCivil(model.Project.ProjectUrl).Verdict == false)
            {
                ModelState.AddModelError("Project.ProjectUrl",
                    "There is an error with your URL, please check it and try again.");
                error = true;
            }

            if (_civilityService.IsCivil(model.Project.Description).Verdict == false)
            {
                ModelState.AddModelError("Project.Description",
                    "There is an error with your Description, please check it and try again.");
                error = true;
            }

            if (!categoryList.Any() ||
                categoryList.Count > enumSelectListItems.Count)
            {
                ModelState.AddModelError("Project.Categories",
                    "Please make sure you have at least one category.");
                error = true;
            }

            foreach (var projectCategory in categoryList)
                if (_civilityService.IsCivil(projectCategory).Verdict == false)
                {
                    ModelState.AddModelError("Project.Categories",
                        "There is an error with at least one of your category name, please check it and try again.");
                    error = true;
                    break;
                }

            newProject.Categories = categoryList;

            var masterCategoryList = enumSelectListItems.Select(e => e.Text).ToList();
            var numberOfDifferences = categoryList.Count(cl => masterCategoryList.All(mcl => mcl != cl));
            if (numberOfDifferences > 0) error = true;

            //End validation
            if (error) return StatusCode(400, ModelState.AllErrors());

            newProject.Slug = model.Project.Slug;
            await _projectService.UpdateProjectAsync(newProject);
            await _projectService.RemoveStaleCategoriesAsync(newProject);
            await _projectService.AddProjectCategoriesAsync(newProject);
            await _projectImageService.RemoveStaleProjectImagesAsync(newProject);
            await _projectImageService.AddProjectImagesAsync(newProject, files.ToList());

            return StatusCode(200);
        }

        ModelState.AddModelError("", "An error has occurred, if this persists, please contact the administrator.");
        return StatusCode(400, ModelState.AllErrors());
    }

    #endregion

    private async Task<bool> ProjectExists(Guid id)
    {
        var project = await _projectService.GetProjectAsync(id);
        if (project.Id == new Guid()) return false;
        return true;
    }
}