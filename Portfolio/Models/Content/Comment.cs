#region Imports

using System.ComponentModel.DataAnnotations;
using Portfolio.Enums;

#endregion

namespace Portfolio.Models.Content;

public class Comment
{
    [Key] public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string AuthorId { get; set; } = default!;
    public string? ModeratorId { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters.",
        MinimumLength = 2)]
    [Display(Name = "Comment")]
    public string Body { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public DateTimeOffset? Moderated { get; set; }
    public DateTimeOffset? SoftDeleted { get; set; }
    public bool IsSoftDeleted { get; set; } = false;
    public bool IsModerated { get; set; } = false;

    [StringLength(500, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters.",
        MinimumLength = 2)]
    [Display(Name = "Moderated Comment")]
    public string? ModeratedBody { get; set; }

    public ModerationType? ModerationType { get; set; }

    //Navigational Properties

    //1:M - One post has many comments
    public virtual Post? Post { get; set; }

    //1:1 - One post has one author
    [Display(Name = "Author")] public virtual BlogUser? Author { get; set; }

    //1:1 - One post has one moderator
    [Display(Name = "Moderator")] public virtual BlogUser? Moderator { get; set; }
}