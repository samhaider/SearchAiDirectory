namespace SearchAiDirectory.Shared.Services;

public interface ICommentService
{
    Task<Comment> GetCommentByID(long ID);
    Task<IList<Comment>> GetCommentsByToolID(long toolID);
    Task<IList<Comment>> GetCommentsByUserID(long userID);
    Task AddComment(Comment comment);
    Task ApproveComment(long commentID);
    Task DeleteComment(long ID);
}

public class CommentService(ApplicationDataContext db) : ICommentService
{
    public async Task<Comment> GetCommentByID(long ID)
    {
        return await db.Comments.Include(c => c.Tool).Include(c => c.User).FirstOrDefaultAsync(c => c.ID == ID);
    }

    public async Task<IList<Comment>> GetCommentsByToolID(long toolID)
    {
        return await db.Comments.Where(c => c.ToolID == toolID).Include(c => c.Tool).Include(c => c.User).ToListAsync();
    }

    public async Task<IList<Comment>> GetCommentsByUserID(long userID)
    {
        return await db.Comments.Where(c => c.UserID == userID).Include(c => c.Tool).Include(c => c.User).ToListAsync();
    }

    public async Task AddComment(Comment comment)
    {
        comment.CreatedOn = DateTime.UtcNow;
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
    }

    public async Task ApproveComment(long commentID)
        => await db.Comments.Where(w => w.ID == commentID).ExecuteUpdateAsync(u => u.SetProperty(s => s.Approve, true));

    public async Task DeleteComment(long ID)
    {
        var comment = await db.Comments.FindAsync(ID);
        if (comment != null)
        {
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
        }
    }
}