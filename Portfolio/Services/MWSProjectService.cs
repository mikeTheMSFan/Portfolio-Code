#region Imports

using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Services;

public class MWSProjectService : IMWSProjectService
{
    private readonly ApplicationDbContext _context;

    public MWSProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Add Project

    public async Task AddProjectAsync(Project project)
    {
        try
        {
            await _context.AddAsync(project);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error adding project to the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("********************************************************************");
            throw;
        }
    }

    #endregion

    #region Add Project Categories

    public async Task AddProjectCategoriesAsync(Project project)
    {
        try
        {
            foreach (var projectCategory in project.Categories)
                await _context.AddAsync(new ProjectCategory
                {
                    ProjectId = project.Id,
                    Text = projectCategory
                });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error creating project categories in the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*********************************************************************************");
            throw;
        }
    }

    #endregion

    #region Delete Project

    public async Task DeleteProjectAsync(Project project)
    {
        try
        {
            _context.Remove(project);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error deleting project in the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("**********************************************************************");
            throw;
        }
    }

    #endregion

    #region Get all Projects

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        var result = await _context.Projects
            .Include(p => p.ProjectCategories)
            .Include(p => p.ProjectImages)
            .OrderByDescending(pr => pr.Created)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get Project

    public async Task<Project> GetProjectAsync(Guid projectId)
    {
        var result = await _context.Projects
            .Include(p => p.ProjectCategories)
            .Include(p => p.ProjectImages)
            .FirstOrDefaultAsync(pr => pr.Id == projectId);

        return result ?? new Project();
    }

    #endregion

    #region Get Project By Slug

    public async Task<Project> GetProjectBySlugAsync(string slug)
    {
        var result = await _context.Projects
            .Include(pj => pj.ProjectImages)
            .Include(pj => pj.ProjectCategories)
            .FirstOrDefaultAsync(pj => pj.Slug!.ToLower() == slug.ToLower());

        return result ?? new Project();
    }

    #endregion

    #region Get top 4 Projects

    public async Task<List<Project>> GetTop4ProjectsAsync()
    {
        var result = await (await GetAllProjectsAsync())
            .OrderByDescending(p => p.Created)
            .Take(4).ToListAsync();

        return result;
    }

    #endregion

    #region Is Project Unique

    public async Task<bool> IsUniqueAsync(string slug)
    {
        var projects = await GetAllProjectsAsync();
        var result = projects.All(pj => pj.Slug!.ToLower() != slug.ToLower());
        return result;
    }

    #endregion

    #region Remove Stale Categories

    public async Task RemoveStaleCategoriesAsync(Project project)
    {
        try
        {
            if (project.ProjectCategories != null) _context.ProjectCategories.RemoveRange(project.ProjectCategories);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing range of project categories**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("**********************************************************************");
            throw;
        }
    }

    #endregion

    #region Update Project

    public async Task UpdateProjectAsync(Project project)
    {
        try
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("****************Error updating project in the database****************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("**********************************************************************");
            throw;
        }
    }

    #endregion
}