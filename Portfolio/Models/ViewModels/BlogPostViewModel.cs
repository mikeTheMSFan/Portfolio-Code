#region Imports

using Portfolio.Models.Content;
using Portfolio.Models.Filters;
using X.PagedList;

#endregion

namespace Portfolio.Models.ViewModels;

public class BlogPostViewModel
{
    public IPagedList<Post> PaginatedPosts { get; set; } = default!;
    public List<Post> RecentArticles { get; set; } = default!;
    public List<Tag>? Tags { get; set; } = default!;
    public Blog Blog { get; set; } = default!;
    public Category? Category { get; set; }
    public string CurrentAction { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
}