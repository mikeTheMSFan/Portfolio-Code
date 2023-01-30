#region Imports

using System.ComponentModel.DataAnnotations;

#endregion

namespace Portfolio.Enums;

public enum ProjectCategories
{
    [Display(Name = "PROJECTS")] Projects,
    [Display(Name = "CHALLENGES")] Challenges,
    [Display(Name = "DOT-NET")] Net,
    [Display(Name = "HTML-5")] Html
}