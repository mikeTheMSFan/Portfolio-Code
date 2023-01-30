#region Imports

using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models.Content;
using Portfolio.Services.Interfaces;
using X.PagedList;

#endregion

namespace Portfolio.Services;

public class MWSCommentService : IMWSCommentService
{
    private readonly ApplicationDbContext _context;

    public MWSCommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Add Comment

    public async Task AddCommentAsync(Comment comment)
    {
        try
        {
            await _context.AddAsync(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error adding comment to the database**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("****************************************************************");
            throw;
        }
    }

    #endregion

    #region Archive Comment

    public async Task ArchiveCommentAsync(Comment comment)
    {
        try
        {
            if (!comment.IsSoftDeleted) comment.IsSoftDeleted = true;

            _context.Update(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("************Error archiving comment in the database************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***************************************************************");
            throw;
        }
    }

    #endregion

    #region Delete Comment

    public async Task DeleteCommentAsync(Comment comment)
    {
        try
        {
            _context.Remove(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("**************Error deleting comment from the database**************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("********************************************************************");
            throw;
        }
    }

    #endregion

    #region Get approved Comments

    public async Task<List<Comment>> GetApprovedCommentsAsync()
    {
        var comments = await GetCommentsAsync();
        var result = await comments
            .Where(c => c.IsSoftDeleted == false)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get Comment by Id

    public async Task<Comment> GetCommentAsync(Guid commentId)
    {
        var result = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Moderator)
            .FirstOrDefaultAsync(c => c.Id == commentId)!;

        return result ?? new Comment();
    }

    #endregion

    #region Get all Comments

    public async Task<List<Comment>> GetCommentsAsync()
    {
        var result = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Moderator)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get moderated Comments

    public async Task<List<Comment>> GetModeratedCommentsAsync()
    {
        var result = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Moderator)
            .Where(c => c.IsModerated == true)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Get sofe-deleted Comments

    public async Task<List<Comment>> GetSoftDeletedComments()
    {
        var result = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Moderator)
            .Where(c => c.IsSoftDeleted == true)
            .ToListAsync();

        return result;
    }

    #endregion

    #region Restore soft-deleted Comments

    public async Task RestoreSoftDeletedCommentAsync(Comment comment)
    {
        if (comment.IsSoftDeleted)
        {
            comment.IsSoftDeleted = false;
            comment.SoftDeleted = null;
            try
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************Error restoring soft-deleted comment in the database*************");
                Console.WriteLine(ex.Message);
                Console.WriteLine("******************************************************************************");
                throw;
            }
        }
    }

    #endregion

    #region Soft-delete comment

    public async Task SoftDeleteCommentAsync(Comment comment)
    {
        if (comment.IsModerated == false)
        {
            comment.IsModerated = true;
            comment.Moderated ??= DateTimeOffset.Now;
        }

        if (comment.IsSoftDeleted == false)
        {
            comment.IsSoftDeleted = true;
            comment.SoftDeleted ??= DateTimeOffset.Now;
        }

        try
        {
            _context.Update(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************Error soft-deleting comment in the database*************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("*********************************************************************");
            throw;
        }
    }

    #endregion

    #region Update Comment

    public async Task UpdateCommentAsync(Comment comment)
    {
        try
        {
            _context.Update(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************Error updating comment in the database*************");
            Console.WriteLine(ex.Message);
            Console.WriteLine("****************************************************************");
            throw;
        }
    }

    #endregion
}