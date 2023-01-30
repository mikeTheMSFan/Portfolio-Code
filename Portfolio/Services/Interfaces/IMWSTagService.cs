#region Imports

using Portfolio.Models.Content;
using Portfolio.Models.Filters;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSTagService
{
    public Task AddTagsAsync(Post post, List<string> tagValues);
    public Task<List<Tag>> GetTopTwentyBlogTagsAsync(Guid blogId);
    public Task RemoveStaleTagsAsync(Post post);
}