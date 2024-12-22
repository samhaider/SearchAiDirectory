namespace SearchAiDirectory.Shared.Services;

public interface ILikeService
{
    Task<bool> HasUserLikedTool(long userId, long toolId);
    Task<(bool IsLiked, long LikeCount)> ToggleLike(long userId, long toolId);
}

public class LikeService(ApplicationDataContext db) : ILikeService
{
    public async Task<bool> HasUserLikedTool(long userId, long toolId)
        => await db.Likes.AnyAsync(l => l.UserID == userId && l.ToolID == toolId);

    public async Task<(bool IsLiked, long LikeCount)> ToggleLike(long userId, long toolId)
    {
        var tool = await db.Tools.FindAsync(toolId) ?? throw new Exception("Tool not found");
        var existingLike = await db.Likes.FirstOrDefaultAsync(l => l.UserID == userId && l.ToolID == toolId);

        if (existingLike is not null)
        {
            // Unlike
            db.Likes.Remove(existingLike);
            tool.LikeCount = Math.Max(0, tool.LikeCount - 1); // Ensure we don't go below 0
            db.Tools.Update(tool);
            await db.SaveChangesAsync();
            return (false, tool.LikeCount);
        }
        else
        {
            // Like
            var like = new Like
            {
                UserID = userId,
                ToolID = toolId
            };
            await db.Likes.AddAsync(like);
            tool.LikeCount++;
            db.Tools.Update(tool);
            await db.SaveChangesAsync();
            return (true, tool.LikeCount);
        }
    }
}