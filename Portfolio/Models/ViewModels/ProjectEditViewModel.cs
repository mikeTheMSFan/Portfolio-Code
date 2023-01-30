#region Imports

using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class ProjectEditViewModel
{
    public Project Project { get; set; } = new();
    public List<SelectListItem>? ProjectSelectListItems { get; set; } = default!;
    public List<Dictionary<string, string>> Base64Images { get; set; } = new();
}