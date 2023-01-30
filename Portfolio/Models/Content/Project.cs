#region Imports

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Portfolio.Models.Filters;

#endregion

namespace Portfolio.Models.Content;

public class Project
{
    [Key] [JsonIgnore] public Guid Id { get; set; }

    [Required]
    [StringLength(60, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters long.",
        MinimumLength = 5)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(200, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters long.",
        MinimumLength = 15)]
    public string Description { get; set; } = default!;

    [DataType(DataType.Date)]
    [Display(Name = "Create Date")]
    public DateTimeOffset? Created { get; set; }

    [Required] public string ProjectUrl { get; set; } = default!;
    public string? Slug { get; set; } = default!;
    [NotMapped] public List<string> Categories { get; set; } = default!;

    //Navigation Properties
    public virtual ICollection<ProjectCategory>? ProjectCategories { get; set; }
    public virtual ICollection<ProjectImage>? ProjectImages { get; set; }
}