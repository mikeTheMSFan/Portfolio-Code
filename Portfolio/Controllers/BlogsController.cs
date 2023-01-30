#nullable disable

#region Imports

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Enums;
using Portfolio.Extensions;
using Portfolio.Models;
using Portfolio.Models.ViewModels;
using Portfolio.Services.Interfaces;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using X.PagedList;

#endregion

namespace Portfolio.Controllers;

[Breadcrumb("Blogs")]
public class BlogsController : Controller
{
    private readonly IMWSBlogService _blogService;
    private readonly IMWSCategoryService _categoryService;
    private readonly IMWSCivilityService _civilityService;
    private readonly IMWSImageService _imageService;
    private readonly IMWSPostService _postService;
    private readonly IMWSTagService _tagService;
    private readonly UserManager<BlogUser> _userManager;

    public BlogsController(UserManager<BlogUser> userManager,
        IMWSImageService imageService,
        IMWSBlogService blogService,
        IMWSCivilityService civilityService,
        IMWSCategoryService categoryService,
        IMWSTagService tagService,
        IMWSPostService postService)
    {
        _userManager = userManager;
        _imageService = imageService;
        _blogService = blogService;
        _civilityService = civilityService;
        _categoryService = categoryService;
        _tagService = tagService;
        _postService = postService;
    }

    [TempData] public string StatusMessage { get; set; } = default!;

    #region All Blogs get action

    [HttpGet]
    [Route("/Blogs/")]
    [Route("/Blogs/Page/{page}")]
    public async Task<IActionResult> AllBlogs(int? page)
    {
        var pageNumber = page ?? 1;
        var pageSize = 6;

        var blogs = (await _blogService.GetAllBlogsAsync())
            .Where(b => b.Posts.Any(p => p.ReadyStatus == ReadyStatus.ProductionReady));

        return View("Index", await blogs.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    #region Blog articles get action

    public IActionResult Articles()
    {
        return NotFound();
    }

    #endregion

    #region Blog search post action

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlogSearch(string term, int? page)
    {
        var pageNumber = page ?? 1;
        var pageSize = 6;
        var blogs = await _blogService.GetBlogsBySearchTerm(term);
        return View("Index", await blogs.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    #region Blog create get action

    [Authorize(Roles = "Administrator")]
    public IActionResult Create()
    {
        var model = new BlogCreateEditViewModel();

        return View(model);
    }

    #endregion

    #region Blog create post action

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] BlogCreateEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "All blogs must have an image.");
                model = GetBlogCreateEditViewModelData(model);
                return View(model);
            }

            model.Blog.Image = await _imageService.EncodeImageAsync(model.ImageFile);
            model.Blog.ImageType = model.ImageFile.ContentType;

            var error = _civilityService.IsCivil(model.Blog.Description).Verdict == false ||
                        _civilityService.IsCivil(model.Blog.Name).Verdict == false;

            if (model.CategoryValues is not null)
            {
                var newCategoryEntries =
                    await _categoryService.RemoveDuplicateCategoriesAsync(model.Blog.Id, model.CategoryValues);

                foreach (var category in newCategoryEntries)
                    if (_civilityService.IsCivil(category).Verdict == false ||
                        (await _categoryService.IsCategoryUnique(model.Blog.Id, category) == false &&
                         category != "All Posts"))
                    {
                        error = true;
                        break;
                    }
            }

            if (error)
            {
                ModelState.AddModelError("",
                    "There is an error, please check entries for bag language, and make sure all required fields are complete.");
                model = GetBlogCreateEditViewModelData(model);
                return View(model);
            }

            model.Blog.Created = DateTimeOffset.Now;
            model.Blog.AuthorId = _userManager.GetUserId(User)!;
            model.Blog.Slug = model.Blog.Name.Slugify();

            if (model.Blog?.Slug is not null &&
                _blogService.IsSlugUniqueAsync(model.Blog.Slug) == false)
            {
                ModelState.AddModelError("Blog.Name", "Your blog name is taken, please choose another.");
                model = GetBlogCreateEditViewModelData(model);
                return View(model);
            }

            await _blogService.AddBlogAsync(model.Blog);
            model.CategoryValues?.Add("All Posts");
            if (model?.CategoryValues is not null)
                await _categoryService.AddCategoriesAsync(model.Blog.Id, model.CategoryValues);

            return RedirectToAction(nameof(Index));
        }

        //Model state is not valid, return user to create view.
        model = GetBlogCreateEditViewModelData(model);
        ModelState.AddModelError("",
            "There has been an error if this continues, please contact the administrator.");

        return View(model);
    }

    #endregion

    #region Blog delete action

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if ((await _blogService.GetBlogAsync(id)).Id == new Guid()) return NotFound();
        var blog = await _blogService.GetBlogAsync(id);
        return View(blog);
    }

    #endregion

    #region Category delete get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [Route("/Blogs/DeleteCategory/{name}/Blog/{blogId}")]
    public async Task<IActionResult> DeleteCategory(string name, Guid blogId)
    {
        if (blogId == new Guid() || blogId == Guid.Empty ||
            (await _blogService.GetBlogAsync(blogId)).Id == new Guid()) return NotFound();

        var category = await _categoryService.GetCategoryByNameAsync(blogId, name);
        if (category.Id == new Guid() || category.Id == Guid.Empty) return NotFound();

        if (category.Name.ToLower() == "all posts") return RedirectToAction("Edit", new { id = category.BlogId });

        return View(category);
    }

    #endregion

    #region Category delete post action

    [HttpPost]
    [ActionName("DeleteCategory")]
    [Route("/Blogs/DeleteCategory/{id}/Blog/{blogId}")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirm(Guid id, Guid blogId)
    {
        if (blogId == new Guid() || blogId == Guid.Empty || id == new Guid() || id == Guid.Empty) return NotFound();
        var categoryToDelete = await _categoryService.GetCategoryByIdAsync(blogId, id);
        if (categoryToDelete.Id == new Guid()) return NotFound();

        await _categoryService.MovePostsToDefaultCategoryAsync(blogId, categoryToDelete);
        await _categoryService.DeleteCategoryAsync(categoryToDelete);

        return RedirectToAction("Edit", new { id = blogId });
    }

    #endregion

    #region Blog delete post action

    [HttpPost]
    [ActionName("Delete")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if ((await _blogService.GetBlogAsync(id)).Id == new Guid()) return NotFound();

        var blog = await _blogService.GetBlogAsync(id);
        await _blogService.DeleteBlogAsync(blog);

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Blog details get action

    [HttpGet]
    [Route("/Blog/{slug}")]
    [Route("/Blog/{slug}/Page/{page}")]
    public async Task<IActionResult> Details(string slug, int? page)
    {
        if ((await _blogService.GetBlogBySlugAsync(slug)).Id == new Guid()) return NotFound();
        var model = new BlogPostViewModel
        {
            Blog = await _blogService.GetBlogBySlugAsync(slug)
        };

        var blogsNode = new MvcBreadcrumbNode("AllBlogs", "Blogs", "Blogs");

        var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Blog.Name)
        {
            RouteValues = new { slug = model.Blog.Slug },
            Parent = blogsNode
        };

        ViewData["BreadcrumbNode"] = blogNode;

        var productionReadyArticles =
            await model.Blog.Posts.Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).ToListAsync();

        var pageNumber = page ?? 1;
        var pageSize = 3;

        model.PaginatedPosts = await productionReadyArticles.ToPagedListAsync(pageNumber, pageSize);
        model.Tags = await _tagService.GetTopTwentyBlogTagsAsync(model.Blog.Id);
        model.CurrentAction = "Details";

        model.RecentArticles = await model.PaginatedPosts.OrderByDescending(p => p.Created)
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .Take(4).ToListAsync();

        return View(model);
    }

    #endregion

    #region Blog category details get action

    [HttpGet]
    [Route("/Blog/{slug}/Category/{categoryName}/Page/{page}")]
    [Breadcrumb("Category")]
    public async Task<IActionResult> DetailsByCategory(string slug, int? page, string categoryName)
    {
        var pageNumber = page ?? 1;
        var pageSize = 3;
        var model = new BlogPostViewModel();

        if (ModelState.IsValid)
        {
            if ((await _blogService.GetBlogBySlugAsync(slug)).Id == new Guid()) return NotFound();
            if (categoryName is null) return NotFound();

            model.Blog = await _blogService.GetBlogBySlugAsync(slug);

            var blogsNode = new MvcBreadcrumbNode("AllBlogs", "Blogs", "Blogs");

            var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Blog.Name)
            {
                RouteValues = new { slug = model.Blog.Slug },
                Parent = blogsNode
            };

            var categoryNode = new MvcBreadcrumbNode("DetailsByCategory", "Blogs", categoryName)
            {
                RouteValues = new { slug, page, categoryName },
                Parent = blogNode
            };

            ViewData["BreadcrumbNode"] = categoryNode;

            model.RecentArticles = await (await _postService.GetPostsByBlogId(model.Blog.Id))
                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created).Take(4).ToListAsync();

            model.CategoryName = categoryName;
            model.Category = await _categoryService.GetCategoryByNameAsync(model.Blog.Id, categoryName);

            model.CurrentAction = "DetailsByCategory";

            model.Tags = await _tagService.GetTopTwentyBlogTagsAsync(model.Blog.Id);

            model.PaginatedPosts =
                await (await _postService.GetPostsByCategory(model.Blog.Id, model.Category))
                    .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                    .ToPagedListAsync(
                        pageNumber, pageSize);

            return View("Details", model);
        }

        TempData["StatusMessage"] = "There is an error, if this continues, please contact the administrator.";
        return View("Details", model);
    }

    #endregion

    #region Blog edit get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if ((await _blogService.GetBlogAsync(id)).Id == new Guid()) return NotFound();

        var model = new BlogCreateEditViewModel
        {
            Blog = await _blogService.GetBlogAsync(id)
        };

        model.CategoryValues = model.Blog.Categories!.Select(c => c.Name).OrderBy(c => c).ToList();
        model.AuthorId = _userManager.GetUserId(User);

        return View(model);
    }

    #endregion

    #region Blog edit post action

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] BlogCreateEditViewModel model)
    {
        if ((await _blogService.GetBlogAsync(id)).Id == new Guid()) return NotFound();
        var blogToUpdate = await _blogService.GetBlogAsync(id);

        if (ModelState.IsValid)
        {
            blogToUpdate!.Updated = DateTimeOffset.Now;
            blogToUpdate.Created = model.Blog.Created;
            blogToUpdate.Name = model.Blog.Name;
            blogToUpdate.Description = model.Blog.Description;
            blogToUpdate.AuthorId = model.Blog.AuthorId;

            if (model.ImageFile is not null)
            {
                model.Blog.ImageType = model.ImageFile!.ContentType;
                model.Blog.Image = await _imageService.EncodeImageAsync(model.ImageFile);
                blogToUpdate.ImageType = model.Blog.ImageType;
                blogToUpdate.Image = model.Blog.Image;
            }

            var error = _civilityService.IsCivil(model.Blog.Name).Verdict == false ||
                        _civilityService.IsCivil(model.Blog.Description).Verdict == false;

            var newCategoryEntries = new List<string>();
            if (model.CategoryValues is not null)
            {
                newCategoryEntries =
                    await _categoryService.RemoveDuplicateCategoriesAsync(model.Blog.Id, model.CategoryValues);

                foreach (var category in newCategoryEntries)
                    if (_civilityService.IsCivil(category).Verdict == false ||
                        (await _categoryService.IsCategoryUnique(model.Blog.Id, category) == false &&
                         category != "All Posts"))
                    {
                        error = true;
                        break;
                    }
            }

            if (error)
            {
                ModelState.AddModelError("",
                    "There is an error, please check entries for bag language, and make sure all required fields are complete.");
                model = GetBlogCreateEditViewModelData(model);
                return View(model);
            }

            model.Blog.Slug = model.Blog.Name.Slugify();
            if (blogToUpdate.Slug != model.Blog.Slug &&
                _blogService.IsSlugUniqueAsync(model.Blog.Slug!) == false)
            {
                model = GetBlogCreateEditViewModelData(model);
                ModelState.AddModelError("", "Your blog name is taken, please choose another.");
                return View(model);
            }

            blogToUpdate.Slug = model.Blog.Slug;

            await _blogService.UpdateBlogAsync(blogToUpdate);
            if (newCategoryEntries.Any())
            {
                await _categoryService.RemoveStaleCategories(model.Blog);
                await _categoryService.AddCategoriesAsync(id, newCategoryEntries);
            }

            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("",
            "There has been an error if this continues, please contact the administrator.");
        model = GetBlogCreateEditViewModelData(model);
        return View(model);
    }

    #endregion

    #region Blog author index get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAuthorBlogs(int? page)
    {
        var pageNumber = page ?? 1;
        var pageSize = 6;
        var currentUserId = _userManager.GetUserId(User);
        var blogs = await _blogService.GetBlogsByAuthorAsync(currentUserId!);

        return View("Index", await blogs.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    #region Blog categories get action

    [HttpGet]
    public async Task<IActionResult> GetCategories(Guid id)
    {
        var categoryList = await _categoryService.GetCategoriesAsync(id);
        return Json(new { categoryListJson = categoryList });
    }

    #endregion

    #region Blog index get action

    [HttpGet]
    public IActionResult Index()
    {
        return NotFound();
    }

    #endregion

    #region Blog post-search get action

    [HttpGet]
    [Route("Blogs/PostSearch")]
    [Route("Blogs/{slug}/PostSearch/{term}/Page/{page}")]
    public async Task<IActionResult> PostSearch(string term, string slug, int? page)
    {
        var model = new BlogPostViewModel();
        var pageNumber = page ?? 1;
        var pageSize = 3;

        if (term == null) return BadRequest();

        model.Blog = await _blogService.GetBlogBySlugAsync(slug);

        if (model.Blog.Id == new Guid()) return NotFound();

        var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Blog.Name)
        {
            RouteValues = new { slug = model.Blog.Slug }
        };

        var searchNode = new MvcBreadcrumbNode("PostSearch", "Blogs", term)
        {
            RouteValues = new { term, slug, page },
            Parent = blogNode
        };

        ViewData["BreadcrumbNode"] = searchNode;

        model.PaginatedPosts = await model.Blog.Posts
            .Where(p => p.Title.ToLower().Contains(term.ToLower()))
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .ToPagedListAsync(pageNumber, pageSize);

        model.RecentArticles = await (await _postService.GetPostsByBlogId(model.Blog.Id))
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .OrderByDescending(p => p.Created).Take(4)
            .ToListAsync();

        model.Tags = await _tagService.GetTopTwentyBlogTagsAsync(model.Blog.Id);
        model.CurrentAction = "PostSearch";
        ViewBag.SearchTerm = term;

        return View("Details", model);
    }

    #endregion

    #region Blog post-search post action

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("PostSearch")]
    public async Task<IActionResult> PostSearchPost(string term, string slug, int? page)
    {
        var model = new BlogPostViewModel();
        var pageNumber = page ?? 1;
        var pageSize = 3;

        if (term == null) return BadRequest();

        model.Blog = await _blogService.GetBlogBySlugAsync(slug);

        if (model.Blog.Id == new Guid()) return NotFound();

        var blogsNode = new MvcBreadcrumbNode("AllBlogs", "Blogs", "Blogs");

        var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Blog.Name)
        {
            RouteValues = new { slug = model.Blog.Slug },
            Parent = blogsNode
        };

        var searchNode = new MvcBreadcrumbNode("PostSearchPost", "Blogs", term)
        {
            RouteValues = new { term, slug },
            Parent = blogNode
        };

        ViewData["BreadcrumbNode"] = searchNode;

        model.PaginatedPosts = await model.Blog.Posts
            .Where(p => p.Title.ToLower().Contains(term.ToLower()) ||
                        p.Abstract.ToLower().Contains(term.ToLower()))
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .ToPagedListAsync(pageNumber, pageSize);

        model.RecentArticles = await (await _postService.GetPostsByBlogId(model.Blog.Id))
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .OrderByDescending(p => p.Created).Take(4)
            .ToListAsync();

        model.Tags = await _tagService.GetTopTwentyBlogTagsAsync(model.Blog.Id);
        model.CurrentAction = "PostSearch";
        ViewBag.SearchTerm = term;

        return View("Details", model);
    }

    #endregion

    #region Blog Tag get action

    [HttpGet]
    [Route("/Blog/{slug}/Tag/{tag}/Page/{page}")]
    public async Task<IActionResult> Tag(int? page, string tag, string slug)
    {
        var model = new BlogPostViewModel();
        var pageNumber = page ?? 1;
        var pageSize = 3;

        model.Blog = await _blogService.GetBlogBySlugAsync(slug);
        var tagPosts = (await _postService.GetPostsByTag(tag, model.Blog.Id))
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady);

        var blogsNode = new MvcBreadcrumbNode("AllBlogs", "Blogs", "Blogs");

        var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Blog.Name)
        {
            RouteValues = new { slug = model.Blog.Slug },
            Parent = blogsNode
        };

        var tagNode = new MvcBreadcrumbNode("Tag", "Blogs", tag)
        {
            RouteValues = new { tag, slug },
            Parent = blogNode
        };

        ViewData["BreadcrumbNode"] = tagNode;

        model.PaginatedPosts = await tagPosts.ToPagedListAsync(pageNumber, pageSize);
        model.RecentArticles = await _postService.GetTopFivePostsByDateAsync(model.Blog.Id);
        model.Tags = await _tagService.GetTopTwentyBlogTagsAsync(model.Blog.Id);
        model.CurrentAction = "Tag";
        ViewBag.Tag = tag;

        return View("Details", model);
    }

    #endregion

    private BlogCreateEditViewModel GetBlogCreateEditViewModelData(BlogCreateEditViewModel model)
    {
        model.ImageFile = default!;
        model.Blog.Image = null;
        model.Blog.ImageType = null;

        return model;
    }
}