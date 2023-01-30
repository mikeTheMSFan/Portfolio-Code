#region Imports

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Portfolio.Models.Filters;

#endregion

namespace Portfolio.Models.Content;

public class Blog
{
    [Key] public Guid Id { get; set; }

    public string AuthorId { get; set; } = default!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
    public string Description { get; set; } = default!;

    [DataType(DataType.Date)]
    [Display(Name = "Created Date")]
    public DateTimeOffset Created { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Updated Date")]
    public DateTimeOffset? Updated { get; set; }

    public byte[]? Image { get; set; }

    public string? ImageType { get; set; }

    [NotMapped] public string? Base64BlogPicture { get; set; }
    public string? Slug { get; set; }


    //Navigation Properties

    public virtual BlogUser? Author { get; set; }

    // 1:M - One blog has many posts
    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();

    //1:M - One blog has many categories
    public virtual ICollection<Category>? Categories { get; set; } = new HashSet<Category>();
}