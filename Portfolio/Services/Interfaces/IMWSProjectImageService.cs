#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSProjectImageService
{
    public Task AddProjectImagesAsync(Project project, List<IFormFile> files);
    public Task RemoveStaleProjectImagesAsync(Project project);
}