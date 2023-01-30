#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class PostEditViewModel
{
    public List<string>? TagValues { get; set; } = new();
    public string? Tags { get; set; }
    public Post? Post { get; set; } = default!;
    public IFormFile? ImageFile { get; set; }
}