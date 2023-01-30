#region Imports

using Portfolio.Models.Content;
using X.PagedList;

#endregion

namespace Portfolio.Models.ViewModels;

public class PostIndexViewModel
{
    public Post Post { get; set; } = default!;
    public IPagedList<Post> Posts { get; set; } = default!;
    public List<string> BlogTags { get; set; } = default!;
    public List<Comment> Comments { get; set; } = default!;
    public List<Post> RecentArticles { get; set; } = default!;
    public Guid PostId { get; set; }
}