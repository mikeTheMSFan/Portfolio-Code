#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class BlogCreateEditViewModel
{
    public Blog Blog { get; set; } = default!;
    public List<string>? CategoryValues { get; set; }
    public string? AuthorId { get; set; }
    public IFormFile? ImageFile { get; set; } = default!;
}