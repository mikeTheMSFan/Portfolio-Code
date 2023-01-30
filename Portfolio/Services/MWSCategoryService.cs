#region Imports

using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Services;

public class MWSCategoryService : IMWSCategoryService
{
    private readonly IMWSBlogService _blogService;
    private readonly ApplicationDbContext _context;

    public MWSCategoryService(ApplicationDbContext context,
        IMWSBlogService blogService)
    {
        _context = context;
        _blogService = blogService;
    }

    #region Add Categories

    public async Task AddCategoriesAsync(Guid blogId, List<string> categoryValues)
    {
        try
        {
            foreach (var categoryName in categoryValues.OrderBy(c => c))
                await _context.AddAsync(new Category
                {
                    BlogId = blogId,
                    Name = categoryName
                });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("***************Error adding categories to the database***************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*****************************************************************");
            throw;
        }
    }

    #endregion

    #region Delete Category

    public async Task DeleteCategoryAsync(Category category)
    {
        try
        {
            _context.Remove(category);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("***************Error removing category from the database***************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************************************************");
            throw;
        }
    }

    #endregion

    #region Get Categories by Blog id

    public async Task<List<Category>> GetCategoriesAsync(Guid blogId)
    {
        var result = await _context.Categories
            .Where(c => c.BlogId == blogId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get Category by Id

    public async Task<Category> GetCategoryByIdAsync(Guid blogId, Guid categoryId)
    {
        var blog = _context.Blogs
            .Include(b => b.Categories)
            .Include(b => b.Author)
            .FirstOrDefault(b => b.Id == blogId);

        var result = new Category();
        if (blog?.Categories is not null)
        {
            var categories = await blog.Categories.ToListAsync();
            result = categories.FirstOrDefault(c => c.Id == categoryId);
        }

        return result ?? new Category();
    }

    #endregion

    #region Get Category by name

    public async Task<Category> GetCategoryByNameAsync(Guid blogId, string categoryName)
    {
        var blog = await _context.Blogs
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == blogId);

        var result = new Category();
        if (blog?.Categories is not null)
        {
            var categories = await blog.Categories.ToListAsync();
            result = categories.FirstOrDefault(c => c.Name.ToLower() == categoryName.ToLower());
        }

        return result ?? new Category();
    }

    #endregion

    #region Category checker

    public async Task<bool> IsCategoryUnique(Guid blogId, string categoryName)
    {
        var userBlog = await _blogService.GetBlogAsync(blogId);
        userBlog = userBlog == new Blog() ? new Blog() : userBlog;
        if (userBlog.Categories!.Any(p => p.Name.ToLower() == categoryName.ToLower())) return false;
        return true;
    }

    #endregion

    #region Move posts to default Category

    public async Task MovePostsToDefaultCategoryAsync(Guid blogId, Category categoryToDelete)
    {
        var blog = _context.Blogs
            .Include(b => b.Categories)
            .Include(b => b.Posts)
            .FirstOrDefault(b => b.Id == blogId);

        if (blog?.Categories is not null)
        {
            var defaultBlogCategory = blog.Categories.FirstOrDefault(c => c.Name.ToLower() == "all posts");
            var postsToMove = await blog.Posts.Where(p => p.Category == categoryToDelete).ToListAsync();
            try
            {
                foreach (var post in postsToMove)
                {
                    post.CategoryId = defaultBlogCategory!.Id;
                    await _context.AddAsync(post);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("**************Error moving posts to default category***************");
                Console.WriteLine(ex.Message);
                Console.WriteLine("*******************************************************************");
                throw;
            }
        }
    }

    #endregion

    #region Remove duplicate Categories

    public async Task<List<string>> RemoveDuplicateCategoriesAsync(Guid blogId, List<string> categoryValues)
    {
        var blogCategories = await GetCategoriesAsync(blogId);
        var lowerCaseCategories = categoryValues.ConvertAll(d => d.ToLower());

        foreach (var category in blogCategories)
            if (lowerCaseCategories.Contains(category.Name.ToLower()))
                lowerCaseCategories.Remove(category.Name.ToLower());

        return lowerCaseCategories;
    }

    #endregion

    #region Remove stale categories

    public async Task RemoveStaleCategories(Blog blog)
    {
        try
        {
            if (blog.Categories != null) _context.Categories.RemoveRange(blog.Categories);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing range of categories***************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***************************************************************");
            throw;
        }
    }

    #endregion
}