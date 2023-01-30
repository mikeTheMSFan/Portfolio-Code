#nullable disable

#region Imports

using System.Net;
using System.Text.RegularExpressions;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Extensions;
using Portfolio.Models;
using Portfolio.Models.Content;
using Portfolio.Models.ViewModels;
using Portfolio.Services.Interfaces;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using X.PagedList;

#endregion

namespace Portfolio.Controllers;

[Breadcrumb]
public class PostsController : Controller
{
    private readonly IMWSBlogService _blogService;
    private readonly IMWSCategoryService _categoryService;
    private readonly IMWSCivilityService _civilityService;
    private readonly IMWSCommentService _commentService;
    private readonly IMWSImageService _imageService;
    private readonly IMWSOpenGraphService _openGraphService;
    private readonly IMWSPostService _postService;
    private readonly IMWSTagService _tagService;
    private readonly UserManager<BlogUser> _userManager;

    public PostsController(UserManager<BlogUser> userManager,
        IMWSPostService postService,
        IMWSBlogService blogService,
        IMWSImageService imageService,
        IMWSTagService tagService,
        IMWSCommentService commentService,
        IMWSCivilityService civilityService,
        IMWSCategoryService categoryService,
        IMWSOpenGraphService openGraphService)
    {
        _userManager = userManager;
        _postService = postService;
        _blogService = blogService;
        _imageService = imageService;
        _tagService = tagService;
        _commentService = commentService;
        _civilityService = civilityService;
        _categoryService = categoryService;
        _openGraphService = openGraphService;
    }

    [TempData] public string StatusMessage { get; set; } = default!;

    #region Add Comment Post Action

    [ValidateReCaptcha]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPostComment([Bind("Id,PostId,AuthorId,Body")] Comment comment,
        string redirectPostSlug, string redirectBlogSlug)
    {
        if (User.Identity!.IsAuthenticated)
        {
            if (ModelState.ContainsKey("Recaptcha") &&
                ModelState["Recaptcha"]!.ValidationState == ModelValidationState.Invalid)
            {
                StatusMessage = "Error: Error verifying reCaptcha, please try again.";
                TempData["Comment"] = comment.Body;
                return RedirectToAction(nameof(Details), new { slug = redirectPostSlug, blogSlug = redirectBlogSlug });
            }

            if (string.IsNullOrEmpty(comment.Body))
            {
                StatusMessage = "Error: A comment cannot be empty";
                TempData["Comment"] = comment.Body;
                return RedirectToAction(nameof(Details), new { slug = redirectPostSlug, blogSlug = redirectBlogSlug });
            }

            comment.Created = DateTimeOffset.Now;
            await _commentService.AddCommentAsync(comment);
            return RedirectToAction(nameof(Details), new { slug = redirectPostSlug, blogSlug = redirectBlogSlug });
        }

        StatusMessage = "Error: you must be logged in to post a comment.";
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region All author posts get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AllAuthorPosts(int? page)
    {
        var currentUserId = _userManager.GetUserId(User);
        var pageNumber = page ?? 1;
        var pageSize = 10;
        var posts = await _postService.GetPostsByUserIdAsync(currentUserId);

        return View("AuthorIndex", await posts.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    #region Author index action

    public IActionResult AuthorIndex()
    {
        return NotFound();
    }

    #endregion

    #region Post create get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        var model = new PostCreateViewModel();
        var userId = _userManager.GetUserId(User);
        var blog = await _blogService.GetBlogsByAuthorAsync(userId);
        if (blog.Any())
        {
            model.BlogSelectList = new SelectList(blog, "Id", "Name", blog.FirstOrDefault());
            model.CategorySelectList =
                new SelectList(await _categoryService.GetCategoriesAsync(blog.FirstOrDefault()!.Id), "Id", "Name");

            return View(model);
        }

        return NotFound();
    }

    #endregion

    #region Post create post action

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] PostCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "All Posts must have an image.");
                model = await GetPostCreateViewModelData(model);
                return View(model);
            }

            if (!model.ImageFile.IsImage())
            {
                ModelState.AddModelError("ImageFile",
                    "The file must be an image, please check your entry and try again.");
                model = await GetPostCreateViewModelData(model);
                return View(model);
            }

            model.Post!.Slug = model.Post!.Title.Slugify();
            if (model.Post?.Slug is not null &&
                await _postService.IsSlugUniqueAsync(model.Post.Slug, model.Post.BlogId) == false)
            {
                ModelState.AddModelError("Post.Title", "Your post title is taken, please choose another.");
                model = await GetPostCreateViewModelData(model);
                return View(model);
            }

            var slug = model.Post?.Title.Slugify();
            var noHtmlContent = model.Post!.Content.RemoveHtmlTags();
            var error = false;

            if (_civilityService.IsCivil(model.Post.Title).Verdict == false)
            {
                ModelState.AddModelError("Post.Title", "There is foul language in the title, please revise.");
                error = true;
            }

            var noHtmlTagsContent = model.Post.Content.RemoveHtmlTags();
            noHtmlTagsContent = WebUtility.HtmlDecode(Regex.Replace(noHtmlTagsContent, "<[^>]*(>|$)", string.Empty));
            if (_civilityService.IsCivil(noHtmlTagsContent).Verdict == false)
            {
                ModelState.AddModelError("Post.Content",
                    "There is foul language in the body of this post, please revise.");
                error = true;
            }

            if (_civilityService.IsCivil(model.Post.Abstract).Verdict == false)
            {
                ModelState.AddModelError("Post.Abstract",
                    "There is foul language in the abstract section of this post, please revise.");
                error = true;
            }

            if (model.TagValues is not null)
            {
                foreach (var tag in model.TagValues)
                    if (_civilityService.IsCivil(tag).Verdict == false)
                    {
                        ModelState.AddModelError("TagValues",
                            "There is foul language in one of your tags, please revise.");
                        error = true;
                        break;
                    }

                if (error)
                {
                    ModelState.AddModelError("", "There is/are (an) error(s) in the form.");
                    model = await GetPostCreateViewModelData(model);
                    return View(model);
                }

                model.Post.Slug = slug;
                model.Post.Created = DateTimeOffset.Now;

                model.Post.Image = await _imageService.EncodeImageAsync(model.ImageFile);
                model.Post.ThumbNail = await _imageService.CreateThumbnailAsync(model.ImageFile);
                model.Post.ImageType = model.ImageFile.ContentType;
                await _openGraphService.AddOpenGraphPostImageAsync(model.Post, model.ImageFile);
                await _postService.AddPostAsync(model.Post!);
                await _tagService.AddTagsAsync(model.Post!, model.TagValues);
            }

            return RedirectToAction(nameof(Index));
        }

        model = await GetPostCreateViewModelData(model);
        return View(model);
    }

    #endregion

    #region Post delete get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if ((await _postService.GetPostByIdAsync(id)).Id == new Guid()) return NotFound();
        var post = await _postService.GetPostByIdAsync(id);
        return View(post);
    }

    #endregion

    #region Post delete post action

    [HttpPost]
    [ActionName("Delete")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if ((await _postService.GetPostByIdAsync(id)).Id == new Guid()) return NotFound();
        var post = await _postService.GetPostByIdAsync(id);
        await _postService.DeletePostAsync(post);
        _openGraphService.DeleteOpenGraphPostImage(post);
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Post details get action

    [HttpGet]
    public async Task<IActionResult> Details(string slug, string blogSlug)
    {
        if (string.IsNullOrEmpty(slug)) return BadRequest();
        if ((await _postService.GetPostBySlugAsync(slug, blogSlug)).Id == new Guid()) return NotFound();

        var model = new PostIndexViewModel
        {
            Post = await _postService.GetPostBySlugAsync(slug, blogSlug)
        };

        var blogsNode = new MvcBreadcrumbNode("AllBlogs", "Blogs", "Blogs");

        var blogNode = new MvcBreadcrumbNode("Details", "Blogs", model.Post.Blog?.Name)
        {
            RouteValues = new { slug = model.Post.Blog?.Slug },
            Parent = blogsNode
        };

        var postNode = new MvcBreadcrumbNode("Details", "Posts", model.Post.Title)
        {
            RouteValues = new { slug, blogSlug },
            Parent = blogNode
        };

        ViewData["BreadcrumbNode"] = postNode;

        model.PostId = model.Post.Id;
        model.RecentArticles = await _postService.GetTopFivePostsByDateAsync(model.Post.BlogId);
        var tagList = await _tagService.GetTopTwentyBlogTagsAsync(model.Post.BlogId);
        model.BlogTags = await tagList.Select(t => t.Text).Distinct().ToListAsync();

        return View(model);
    }

    #endregion

    #region Post edit get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if ((await _postService.GetPostByIdAsync(id)).Id == new Guid()) return NotFound();

        var model = new PostEditViewModel();
        var userId = _userManager.GetUserId(User);
        var blog = await _blogService.GetBlogsByAuthorAsync(userId);
        model.Post = await _postService.GetPostByIdAsync(id);
        if (blog.FirstOrDefault() is not null)
        {
            if (model.Post?.Tags is not null)
            {
                foreach (var tag in model.Post.Tags) model.TagValues!.Add(tag.Text);

                model.Tags = string.Join(",", model.TagValues!);
            }

            return View(model);
        }

        return NotFound();
    }

    #endregion

    #region Post edit post action

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] PostEditViewModel model)
    {
        if ((await _postService.GetPostByIdAsync(id)).Id == new Guid()) return NotFound();
        if (ModelState.IsValid)
        {
            var newPost = await _postService.GetPostByIdAsync(id);
            if (model.Post is not null)
            {
                newPost.Updated = DateTimeOffset.Now;
                newPost.Content = model.Post.Content;
                newPost.Abstract = model.Post.Abstract;
                newPost.AuthorId = model.Post.AuthorId;
                newPost.CategoryId = model.Post.CategoryId;
                newPost.BlogId = model.Post.BlogId;
                newPost.ReadyStatus = model.Post.ReadyStatus;
                newPost.Title = model.Post.Title;
                newPost.Id = model.Post.Id;
                newPost.Tags = model.Post.Tags;
            }

            if (model.Post!.Image is null)
            {
                model.Post.Image = newPost.Image;
                model.Post.ImageType = newPost.ImageType;
            }

            if (model.ImageFile is not null &&
                !model.ImageFile.IsImage())
            {
                ModelState.AddModelError("ImageFile",
                    "The file must be an image, please check your entry and try again.");
                model = GetPostEditViewModelData(model);
                return View(model);
            }

            if (model.ImageFile is not null)
            {
                newPost.Image = await _imageService.EncodeImageAsync(model.ImageFile);
                newPost.ThumbNail = await _imageService.CreateThumbnailAsync(model.ImageFile);
                newPost.ImageType = model.ImageFile.ContentType;
            }

            var newSlug = model.Post!.Title.Slugify();
            if (await _postService.IsSlugUniqueAsync(newSlug!, model.Post!.BlogId) == false &&
                newSlug != newPost.Slug)
            {
                ModelState.AddModelError("Post.Title", "Your title is already taken, please choose another.");
                model = GetPostEditViewModelData(model);
                return View(model);
            }

            newPost.Slug = newSlug;

            var noHtmlContent = newPost.Content.RemoveHtmlTags();
            noHtmlContent = WebUtility.HtmlDecode(Regex.Replace(noHtmlContent, "<[^>]*(>|$)", string.Empty));
            var error = false;

            if (_civilityService.IsCivil(newPost.Title).Verdict == false)
            {
                ModelState.AddModelError("Post.Title", "There is foul language in the title, please revise.");
                error = true;
            }

            if (_civilityService.IsCivil(noHtmlContent).Verdict == false)
            {
                ModelState.AddModelError("Post.Content",
                    "There is foul language in the body of this post, please revise.");
                error = true;
            }

            if (_civilityService.IsCivil(newPost.Abstract).Verdict == false)
            {
                ModelState.AddModelError("Post.Abstract",
                    "There is foul language in the abstract section of this post, please revise.");
                error = true;
            }


            if (model.TagValues is not null)
                foreach (var tag in model.TagValues)
                    if (_civilityService.IsCivil(tag).Verdict == false)
                    {
                        ModelState.AddModelError("TagValues",
                            "There is foul language in one of your tags, please revise.");
                        error = true;
                        break;
                    }

            if (error)
            {
                ModelState.AddModelError("", "There is/are (an) error(s) in the form.");
                model = GetPostEditViewModelData(model);
                return View(model);
            }

            if (model.ImageFile is not null)
                await _openGraphService.AddOpenGraphPostImageAsync(newPost, model.ImageFile);

            await _postService.UpdatePostAsync(newPost);


            await _tagService.RemoveStaleTagsAsync(newPost);
            await _tagService.AddTagsAsync(newPost, model.TagValues!);

            return RedirectToAction(nameof(Index));
        }

        model = GetPostEditViewModelData(model);
        return View(model);
    }

    #endregion

    #region Post index get action

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Index(int? page)
    {
        var pageNumber = page ?? 1;
        var pageSize = 6;
        var posts = await _postService.GetAllPosts();
        var model = new PostIndexViewModel
        {
            Posts = await posts.ToPagedListAsync(pageNumber, pageSize),
            RecentArticles = await posts.OrderByDescending(p => p.Created).Take(9).ToListAsync()
        };

        return View(model);
    }

    #endregion

    #region Search post action

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(string term, int? page)
    {
        var blogUserId = _userManager.GetUserId(User);
        var pageNumber = page ?? 1;
        var pageSize = 10;
        var posts = await _postService.GetPostsByUserIdAsync(blogUserId!);

        var result = posts
            .Where(p => p.Title.ToLower().Contains(term.ToLower()));

        //return posts to the author index view.
        return View("AuthorIndex", await result.ToPagedListAsync(pageNumber, pageSize));
    }

    #endregion

    private async Task<PostCreateViewModel> GetPostCreateViewModelData(PostCreateViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        var blog = await _blogService.GetBlogsByAuthorAsync(userId!);
        var lastSelectedBlog = await _blogService.GetBlogAsync(model.Post!.BlogId);

        model.BlogSelectList = new SelectList(blog, "Id", "Name", lastSelectedBlog);

        model.CategorySelectList =
            new SelectList(await _categoryService.GetCategoriesAsync(lastSelectedBlog.Id), "Id", "Name");

        model.ImageFile = default!;

        return model;
    }

    private PostEditViewModel GetPostEditViewModelData(PostEditViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        model.ImageFile = default!;

        return model;
    }
}