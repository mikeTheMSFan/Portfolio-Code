#region Imports

using System.ComponentModel.DataAnnotations;
using Portfolio.Models.Content;

#endregion

namespace Portfolio.Models.Filters;

public class ProjectImage
{
    [Key] public Guid Id { get; set; }

    public byte[] File { get; set; } = default!;
    public string FileContentType { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid ProjectId { get; set; }

    //Navigational Properties
    public virtual Project Project { get; set; } = default!;
}