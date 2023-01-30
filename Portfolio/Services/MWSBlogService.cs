#region Imports

using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models.Content;
using Portfolio.Services.Interfaces;

#endregion

namespace Portfolio.Services;

public class MWSBlogService : IMWSBlogService
{
    private readonly ApplicationDbContext _context;

    public MWSBlogService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Add Blog

    public async Task AddBlogAsync(Blog blog)
    {
        try
        {
            await _context.AddAsync(blog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error adding blog to the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion

    #region Delete blog

    public async Task DeleteBlogAsync(Blog blog)
    {
        try
        {
            _context.Remove(blog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error deleting blog from the database**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion

    #region Get All Blogs

    public async Task<List<Blog>> GetAllBlogsAsync()
    {
        var result = await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Categories)
            .Include(b => b.Posts)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get Blog by id

    public async Task<Blog> GetBlogAsync(Guid blogId)
    {
        var result = await _context.Blogs
            .Include(b => b.Categories)
            .Include(b => b.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Tags)
            .FirstOrDefaultAsync(b => b.Id == blogId);

        if (result?.Posts is not null)
            //gets comments that are not soft-deleted
            result.Posts = FilterPostComments(result.Posts);

        return result ?? new Blog();
    }

    #endregion

    #region Get Blog by Slug

    public async Task<Blog> GetBlogBySlugAsync(string slug)
    {
        var result = await _context.Blogs
            .Include(b => b.Categories)
            .Include(b => b.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Author)
            .FirstOrDefaultAsync(b => b.Slug!.ToLower() == slug.ToLower());

        if (result?.Posts is not null)
            //gets comments that are not soft-deleted
            result.Posts = FilterPostComments(result.Posts);

        return result ?? new Blog();
    }

    #endregion

    #region Get All Blogs by Author id

    public async Task<List<Blog>> GetBlogsByAuthorAsync(string authorId)
    {
        var result = await _context.Blogs
            .Include(b => b.Categories)
            .Include(b => b.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)!
            .ThenInclude(c => c.Author)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Author)
            .Where(b => b.AuthorId == authorId)
            .ToListAsync();

        return result ?? new List<Blog>();
    }

    #endregion

    #region Get All Blogs by search term

    public async Task<List<Blog>> GetBlogsBySearchTerm(string term)
    {
        var result = await _context.Blogs
            .Include(b => b.Author)
            .Where(b => b.Name.ToLower().Contains(term.ToLower()))
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get inital Blog

    public async Task<Blog> GetFirstBlog()
    {
        var result = new Blog();
        if (_context.Blogs.Any())
            result = await _context.Blogs
                .Include(b => b.Categories)
                .Include(b => b.Author)
                .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)!
                .ThenInclude(c => c.Author)
                .Include(b => b.Posts)
                .ThenInclude(p => p.Author)
                .Include(b => b.Posts)
                .ThenInclude(p => p.Tags)
                .OrderBy(b => b.Created).FirstAsync();

        if (result?.Posts is not null)
            //gets comments that are not soft-deleted
            result.Posts = FilterPostComments(result.Posts);

        return result ?? new Blog();
    }

    #endregion

    #region Slug checker

    public bool IsSlugUniqueAsync(string slug)
    {
        var result = !_context.Blogs.Any(b => b.Slug!.ToLower() == slug.ToLower());
        return result;
    }

    #endregion

    #region Update Blog

    public async Task UpdateBlogAsync(Blog blog)
    {
        try
        {
            _context.Update(blog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("***************Error updating blog in the database***************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion


    private ICollection<Post> FilterPostComments(ICollection<Post> posts)
    {
        foreach (var post in posts)
            if (post?.Comments is not null)
            {
                var comments = post.Comments.Where(c => c.IsSoftDeleted == false);
                post.Comments = comments.ToList();
            }

        return posts;
    }
}