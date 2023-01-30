#region Imports

using System.ComponentModel.DataAnnotations;
using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.Filters;

public class ProjectCategory
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid ProjectId { get; set; }
    [Required] public string Text { get; set; } = string.Empty;

    //Navigation Properties
    public virtual Project? Project { get; set; } = default!;
}