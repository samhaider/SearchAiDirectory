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

public class CommentService(IDbContextFactory<ApplicationDataContext> dbContextFactory) : ICommentService
{
    public async Task<Comment> GetCommentByID(long ID)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Comments.Include(c => c.Tool).Include(c => c.User).FirstOrDefaultAsync(c => c.ID == ID);
    }

    public async Task<IList<Comment>> GetCommentsByToolID(long toolID)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Comments.Where(c => c.ToolID == toolID).Include(c => c.Tool).Include(c => c.User).ToListAsync();
    }

    public async Task<IList<Comment>> GetCommentsByUserID(long userID)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Comments.Where(c => c.UserID == userID).Include(c => c.Tool).Include(c => c.User).ToListAsync();
    }

    public async Task AddComment(Comment comment)
    {
        using var db = dbContextFactory.CreateDbContext();
        comment.CreatedOn = DateTime.UtcNow;
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
    }

    public async Task ApproveComment(long commentID)
    {
        using var db = dbContextFactory.CreateDbContext();
        await db.Comments.Where(w => w.ID == commentID).ExecuteUpdateAsync(u => u.SetProperty(s => s.Approve, true));
    }

    public async Task DeleteComment(long ID)
    {
        using var db = dbContextFactory.CreateDbContext();
        var comment = await db.Comments.FindAsync(ID);
        if (comment != null)
        {
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
        }
    }
}