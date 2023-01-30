#region Imports

using Portfolio.Data;
using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using Portfolio.Services.Interfaces;

#endregion

namespace Portfolio.Services;

public class MWSProjectImageService : IMWSProjectImageService
{
    private readonly ApplicationDbContext _context;
    private readonly IMWSImageService _imageService;

    public MWSProjectImageService(IMWSImageService imageService, ApplicationDbContext context)
    {
        _imageService = imageService;
        _context = context;
    }

    public async Task AddProjectImagesAsync(Project project, List<IFormFile> files)
    {
        var model = new ProjectImage();
        try
        {
            var projectId = project.Id;

            var i = 1;
            foreach (var file in files.OrderBy(f => f.FileName))
            {
                var projectImageId = Guid.NewGuid();
                model.Id = projectImageId;
                model.ProjectId = projectId;
                model.File = await _imageService.EncodeImageAsync(file);
                model.FileContentType = file.ContentType;
                model.Name = $"{i}";
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();

                i++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error adding project images**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*******************************************************");
            throw;
        }
    }

    public async Task RemoveStaleProjectImagesAsync(Project project)
    {
        try
        {
            if (project.ProjectImages != null)
            {
                _context.ProjectImages.RemoveRange(project.ProjectImages);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error removing range of project images**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("******************************************************************");
            throw;
        }
    }
}