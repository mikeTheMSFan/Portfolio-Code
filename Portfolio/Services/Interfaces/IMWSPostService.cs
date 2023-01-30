#region Imports

using Portfolio.Models.Content;
using Portfolio.Models.Filters;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSPostService
{
    public Task AddPostAsync(Post post);
    public Task DeletePostAsync(Post post);
    public Task<List<Post>> GetAllPosts();
    public Task<Post> GetPostByIdAsync(Guid id);
    public Task<Post> GetPostBySlugAsync(string slug, string blogSlug);
    public Task<List<Post>> GetPostsByBlogId(Guid blogId);
    public Task<List<Post>> GetPostsByCategory(Guid blogId, Category category);
    public Task<List<Post>> GetPostsByTag(string tag, Guid blogId);
    public Task<List<Post>> GetPostsByUserIdAsync(string id);
    public Task<List<Post>> GetTopFivePostsByDateAsync(Guid blogId);
    public Task<bool> IsSlugUniqueAsync(string slug, Guid blogId);
    public Task UpdatePostAsync(Post post);
}