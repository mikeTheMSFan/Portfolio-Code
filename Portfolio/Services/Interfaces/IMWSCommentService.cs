#region Imports

using Portfolio.Models.Content;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSCommentService
{
    public Task AddCommentAsync(Comment comment);
    public Task ArchiveCommentAsync(Comment comment);
    public Task DeleteCommentAsync(Comment comment);
    public Task<List<Comment>> GetApprovedCommentsAsync();
    public Task<Comment> GetCommentAsync(Guid commentId);
    public Task<List<Comment>> GetCommentsAsync();
    public Task<List<Comment>> GetModeratedCommentsAsync();
    public Task<List<Comment>> GetSoftDeletedComments();
    public Task RestoreSoftDeletedCommentAsync(Comment comment);
    public Task SoftDeleteCommentAsync(Comment comment);
    public Task UpdateCommentAsync(Comment comment);
}