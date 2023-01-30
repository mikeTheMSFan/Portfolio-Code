#region Imports

using Portfolio.Data;
using Portfolio.Enums;
using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Services;

public class MWSTagService : IMWSTagService
{
    private readonly IMWSBlogService _blogService;
    private readonly ApplicationDbContext _context;

    public MWSTagService(ApplicationDbContext context,
        IMWSBlogService blogService)
    {
        _context = context;
        _blogService = blogService;
    }

    #region Add Tags

    public async Task AddTagsAsync(Post post, List<string> tagValues)
    {
        foreach (var tag in tagValues)
            try
            {
                await _context.AddAsync(new Tag
                {
                    PostId = post.Id,
                    BlogUserId = post.AuthorId,
                    Text = tag
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("**************Error adding post Tags to the database**************");
                Console.WriteLine(ex.Message);
                Console.WriteLine("*****************************************************************");
                throw;
            }
    }

    #endregion

    #region Get top twenty Tags

    public async Task<List<Tag>> GetTopTwentyBlogTagsAsync(Guid blogId)
    {
        var blog = await _blogService.GetBlogAsync(blogId);
        var posts = await blog.Posts
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .ToListAsync();

        var result = new List<Tag>();
        if (posts is not null)
        {
            var allTags = await posts
                .SelectMany(p => p.Tags ??= new List<Tag>())
                .OrderBy(t => t.Text)
                .GroupBy(t => t.Text)
                .Select(t => t.First())
                .ToListAsync();

            result = (allTags.Count > 20 ? allTags.Take(20) : allTags) as List<Tag>;
        }

        return result ?? new List<Tag>();
    }

    #endregion

    #region Remove stale Tags

    public async Task RemoveStaleTagsAsync(Post post)
    {
        try
        {
            if (post.Tags != null) _context.Tags.RemoveRange(post.Tags);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing range of tags**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("********************************************************");
            throw;
        }
    }

    #endregion
}