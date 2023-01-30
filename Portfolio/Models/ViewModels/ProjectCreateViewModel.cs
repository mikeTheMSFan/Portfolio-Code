#region Imports

using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class ProjectCreateViewModel
{
    public Project Project { get; set; } = new();
    public List<SelectListItem>? ProjectSelectListItems { get; set; } = default!;
}