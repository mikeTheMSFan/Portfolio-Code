#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSBlogService
{
    public Task AddBlogAsync(Blog blog);
    public Task DeleteBlogAsync(Blog blog);
    public Task<List<Blog>> GetAllBlogsAsync();
    public Task<Blog> GetBlogAsync(Guid blogId);
    public Task<Blog> GetBlogBySlugAsync(string slug);
    public Task<List<Blog>> GetBlogsByAuthorAsync(string authorId);
    public Task<List<Blog>> GetBlogsBySearchTerm(string term);
    public Task<Blog> GetFirstBlog();
    public bool IsSlugUniqueAsync(string slug);
    public Task UpdateBlogAsync(Blog blog);
}