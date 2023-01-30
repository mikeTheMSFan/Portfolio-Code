#region Imports

using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class PostCreateViewModel
{
    public SelectList? BlogSelectList { get; set; }
    public SelectList? CategorySelectList { get; set; }
    public List<string>? TagValues { get; set; } = default!;
    public string? Tags { get; set; }
    public Post? Post { get; set; } = default!;
    public IFormFile? ImageFile { get; set; }
}