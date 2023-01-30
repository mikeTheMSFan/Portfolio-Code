#nullable disable

#region Imports

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Content;
using Portfolio.Services.Interfaces;

#endregion

namespace Portfolio.Controllers;

[Authorize]
public class CommentsController : Controller
{
    private readonly IMWSCommentService _commentService;

    public CommentsController(IMWSCommentService commentService)
    {
        _commentService = commentService;
    }

    [TempData] public string StatusMessage { get; set; } = default!;

    #region Approved comments get action

    public async Task<IActionResult> ApprovedComments()
    {
        var comments = await _commentService.GetApprovedCommentsAsync();
        return View(nameof(Index), comments);
    }

    #endregion

    #region Delete comments get action

    // GET: Comments/Delete/5
    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (await CommentExists(id) == false) return NotFound();
        var comment = await _commentService.GetCommentAsync(id);
        return View(comment);
    }

    #endregion

    #region Delete comments post action

    // POST: Comments/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [Authorize(Roles = "Administrator,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (await CommentExists(id) == false) return NotFound();
        var comment = await _commentService.GetCommentAsync(id);
        await _commentService.DeleteCommentAsync(comment);

        //return user to index view with comment view model.
        return View("Index", await _commentService.GetModeratedCommentsAsync());
    }

    #endregion

    #region Edit comments get action

    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (await CommentExists(id) == false) return NotFound();
        var comment = await _commentService.GetCommentAsync(id);
        return View(comment);
    }

    #endregion

    #region Edit comments post action

    [HttpPost]
    [Authorize(Roles = "Administrator,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id,
        [Bind(
            "Id,PostId,AuthorId,ModeratorId,Body,Created,Updated,Moderated,Deleted,ModeratedBody,ModerationType")]
        Comment comment)
    {
        //if comment is not found, return 404 error.
        if (await CommentExists(id) == false) return NotFound();

        if (ModelState.IsValid)
        {
            var newComment = await _commentService.GetCommentAsync(id);
            if (newComment == new Comment()) return NotFound();

            newComment.Id = comment.Id;
            newComment.AuthorId = comment.AuthorId;
            newComment.Body = comment.Body;
            newComment.ModeratorId = comment.ModeratorId;
            newComment.ModerationType = comment.ModerationType;
            newComment.ModeratedBody = comment.ModeratedBody;
            newComment.Updated = DateTimeOffset.Now;
            newComment.IsModerated = true;

            if (comment.IsModerated &&
                comment.Moderated is null)
                newComment.Moderated = DateTimeOffset.Now;

            await _commentService.UpdateCommentAsync(newComment);

            return View("Index", await _commentService.GetModeratedCommentsAsync());
        }

        return View(comment);
    }

    #endregion

    #region Comment index action

    public IActionResult Index()
    {
        return NotFound();
    }

    #endregion

    #region Moderated comments get action

    [HttpGet]
    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> ModeratedIndex()
    {
        return View("Index", await _commentService.GetModeratedCommentsAsync());
    }

    #endregion

    #region Soft deleted comments get action

    [HttpGet]
    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> SDelete(Guid id)
    {
        if (await CommentExists(id) == false) return NotFound();
        var comment = await _commentService.GetCommentAsync(id);
        await _commentService.SoftDeleteCommentAsync(comment);

        return View("Index", await _commentService.GetModeratedCommentsAsync());
    }

    #endregion

    #region Soft delete comments get action

    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> SoftDeletedComments()
    {
        //get soft delete comments
        var softDeletedComments = await _commentService.GetSoftDeletedComments();

        //return comments to the index view.
        return View("Index", softDeletedComments);
    }

    #endregion

    #region Soft delete comments restore get action

    public async Task<IActionResult> SRestore(Guid id)
    {
        if (await CommentExists(id) == false) return NotFound();
        var comment = await _commentService.GetCommentAsync(id);
        await _commentService.RestoreSoftDeletedCommentAsync(comment);

        return View("Index", await _commentService.GetModeratedCommentsAsync());
    }

    #endregion

    private async Task<bool> CommentExists(Guid id)
    {
        var comment = await _commentService.GetCommentAsync(id);
        if (comment == new Comment()) return false;

        return true;
    }
}