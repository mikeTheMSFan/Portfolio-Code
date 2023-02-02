#region Imports

using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Enums;
using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Services;

public class MWSPostService : IMWSPostService
{
    private readonly IMWSBlogService _blogService;
    private readonly ApplicationDbContext _context;
    private readonly IMWSImageService _imageService;

    public MWSPostService(ApplicationDbContext context,
        IMWSBlogService blogService,
        IMWSImageService imageService)
    {
        _context = context;
        _blogService = blogService;
        _imageService = imageService;
    }

    #region Delete Post

    public async Task DeletePostAsync(Post post)
    {
        try
        {
            _context.Remove(post);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing post from the database**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion

    #region Get all Posts

    public async Task<List<Post>> GetAllPosts()
    {
        var result = await _context.Posts
            .Include(p => p.Blog)
            .Include(p => p.Author)
            .Include(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(p => p.Category)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get Post by id

    public async Task<Post> GetPostByIdAsync(Guid id)
    {
        var result = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(p => p.Tags)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return result ?? new Post();
    }

    #endregion

    #region Get Post by slug

    public async Task<Post> GetPostBySlugAsync(string slug, string blogSlug)
    {
        var blog = await _blogService.GetBlogBySlugAsync(blogSlug);

        var result = blog.Posts
            .FirstOrDefault(p => p.Slug!.ToLower() == slug.ToLower());

        if (result is not null) result.Blog = blog;

        return result ?? new Post();
    }

    #endregion

    #region Get Posts by Blog id

    public async Task<List<Post>> GetPostsByBlogId(Guid blogId)
    {
        var blog = _context.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Tags)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Category)
            .FirstOrDefault(b => b.Id == blogId);

        var result = new List<Post>();
        if (blog is not null) result = await blog.Posts.ToListAsync();

        return result;
    }

    #endregion

    #region Get Posts by category

    public async Task<List<Post>> GetPostsByCategory(Guid blogId, Category category)
    {
        var blog = _context.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Tags)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Category)
            .FirstOrDefault(b => b.Id == blogId);

        var result = new List<Post>();
        if (blog?.Posts is not null)
            result = await blog.Posts.Where(p => p.Category == category && p.ReadyStatus == ReadyStatus.ProductionReady)
                .ToListAsync();

        return result ?? new List<Post>();
    }

    #endregion

    #region Get Posts by tag

    public async Task<List<Post>> GetPostsByTag(string tag, Guid blogId)
    {
        var blog = await _blogService.GetBlogAsync(blogId);
        var posts = await blog.Posts
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .ToListAsync();

        var result = new List<Post>();
        if (posts is not null)
            result = await posts
                .Where(p => p.Tags != null && p.Tags.Any(t => t.Text == tag))
                .ToListAsync();

        return result;
    }

    #endregion

    #region Get Posts by User id

    public async Task<List<Post>> GetPostsByUserIdAsync(string id)
    {
        var result = await _context.Posts
            .Include(p => p.Blog)
            .Include(p => p.Author)
            .Include(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(p => p.Category)
            .Where(p => p.AuthorId == id)
            .ToListAsync();

        return result!;
    }

    #endregion

    #region Get top five Posts by date

    public async Task<List<Post>> GetTopFivePostsByDateAsync(Guid blogId)
    {
        var blog = await _blogService.GetBlogAsync(blogId);

        var result = await blog.Posts
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .OrderByDescending(p => p.Created)
            .Take(5)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Slug checker

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid blogId)
    {
        var blog = await _blogService.GetBlogAsync(blogId);
        var posts = blog.Posts;

        var result = posts.All(p => p.Slug!.ToLower() != slug.ToLower());
        return result;
    }

    #endregion

    #region Update Post

    public async Task UpdatePostAsync(Post post)
    {
        try
        {
            _context.Update(post);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing post from the database**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion

    #region Add Post

    public async Task AddPostAsync(Post post)
    {
        try
        {
            await _context.AddAsync(post);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error adding post to the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion
}