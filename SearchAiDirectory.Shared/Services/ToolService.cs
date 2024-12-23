namespace SearchAiDirectory.Shared.Services;

public interface IToolService
{
    Task<IList<Tool>> GetAllTools();
    Task<bool> ToolExists(string name);
    Task<Tool> GetToolByID(long toolID);
    Task<Tool> GetToolByName(string toolName);
    Task<Tool> GetToolBySlug(string toolSlug);
    Task<long> AddTool(Tool newTool);
    Task UpdateTool(Tool updatedTool);
    Task ChangeCategory(long toolID, long categoryID);
    Task UpdateToolImageUrl(long toolID, string imageUrl);
    Task<IList<Tool>> GetTop3Tools();
}

public class ToolService(IDbContextFactory<ApplicationDataContext> dbContextFactory) : IToolService
{
    public async Task<IList<Tool>> GetAllTools()
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools.ToListAsync();
    }

    public async Task<bool> ToolExists(string name)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools.AnyAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Tool> GetToolByID(long toolID)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools
            .Where(w => w.ID == toolID)
            .Include(i => i.Category)
            .Include(i => i.Likes)
            .Include(i => i.Comments)
            .SingleOrDefaultAsync();
    }

    public async Task<Tool> GetToolByName(string toolName)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools.Include(i => i.Category).Where(w => w.Name.ToLower() == toolName.ToLower()).SingleOrDefaultAsync();
    }

    public async Task<Tool> GetToolBySlug(string toolSlug)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools
            .Where(db => db.Slug == toolSlug)
            .Include(i => i.Category)
            .Include(i => i.Likes)
            .Include(i => i.Comments).ThenInclude(i => i.User)
            .Include(i => i.Embedding)
            .SingleOrDefaultAsync();
    }

    public async Task<long> AddTool(Tool newTool)
    {
        using var db = dbContextFactory.CreateDbContext();
        await db.Tools.AddAsync(newTool);
        await db.SaveChangesAsync();
        return newTool.ID;
    }

    public async Task UpdateTool(Tool updateTool)
    {
        using var db = dbContextFactory.CreateDbContext();
        updateTool.Modified = DateTime.UtcNow;
        db.Tools.Update(updateTool);
        await db.SaveChangesAsync();
    }

    public async Task UpdateToolImageUrl(long toolID, string imageUrl)
    {
        using var db = dbContextFactory.CreateDbContext();
        await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageUrl, imageUrl));
    }

    public async Task<IList<Tool>> GetTop3Tools()
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.Tools.OrderBy(o => o.Created).Take(3).ToListAsync();
    }

    public async Task ChangeCategory(long toolID, long categoryID)
    {
        using var db = dbContextFactory.CreateDbContext();
        await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));
    }
}