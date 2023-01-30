#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSProjectService
{
    public Task AddProjectAsync(Project project);
    public Task AddProjectCategoriesAsync(Project project);
    public Task DeleteProjectAsync(Project project);
    public Task<List<Project>> GetAllProjectsAsync();
    public Task<Project> GetProjectAsync(Guid projectId);
    public Task<Project> GetProjectBySlugAsync(string slug);
    public Task<List<Project>> GetTop4ProjectsAsync();
    public Task<bool> IsUniqueAsync(string slug);
    public Task RemoveStaleCategoriesAsync(Project project);
    public Task UpdateProjectAsync(Project project);
}