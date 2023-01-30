#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSOpenGraphService
{
    public Task AddOpenGraphPostImageAsync(Post post, IFormFile file);
    public Task AddOpenGraphProjectImageAsync(Project project, IFormFile file);
    public void DeleteOpenGraphPostImage(Post post);
    public void DeleteOpenGraphProjectImage(Project project);
}