#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.ViewModels;

public class FrontPageViewModel
{
    public List<Project> Projects { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
    public ContactEmail ContactEmail { get; set; } = new();
}