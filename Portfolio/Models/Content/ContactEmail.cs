#region Imports

using System.ComponentModel.DataAnnotations;

#endregion

namespace Portfolio.Models.Content;

public class ContactEmail
{
    [Required]
    [StringLength(65, ErrorMessage = "The {0} must be at least {2} and at most {1}. ", MinimumLength = 2)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1}.", MinimumLength = 15)]
    public string Subject { get; set; } = default!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1}.", MinimumLength = 40)]
    public string Body { get; set; } = default!;
}