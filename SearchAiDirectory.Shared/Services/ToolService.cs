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

public class ToolService(ApplicationDataContext db) : IToolService
{
    public async Task<IList<Tool>> GetAllTools()
        => await db.Tools.ToListAsync();

    public async Task<bool> ToolExists(string name)
        => await db.Tools.AnyAsync(a => a.Name.ToLower() == name.ToLower());

    public async Task<Tool> GetToolByID(long toolID)
        => await db.Tools
        .Where(w => w.ID == toolID)
        .Include(i => i.Category)
        .Include(i => i.Likes)
        .Include(i => i.Comments)
        .SingleOrDefaultAsync();

    public async Task<Tool> GetToolByName(string toolName)
        => await db.Tools.Include(i => i.Category).Where(w => w.Name.ToLower() == toolName.ToLower()).SingleOrDefaultAsync();

    public async Task<Tool> GetToolBySlug(string toolSlug)
        => await db.Tools
        .Where(db => db.Slug == toolSlug)
        .Include(i => i.Category)
        .Include(i => i.Likes)
        .Include(i => i.Comments).ThenInclude(i => i.User)
        .Include(i => i.Embedding)
        .SingleOrDefaultAsync();

    public async Task<long> AddTool(Tool newTool)
    {
        await db.Tools.AddAsync(newTool);
        await db.SaveChangesAsync();
        return newTool.ID;
    }

    public async Task UpdateTool(Tool updateTool)
    {
        updateTool.Modified = DateTime.UtcNow;
        db.Tools.Update(updateTool);
        await db.SaveChangesAsync();
    }

    public async Task UpdateToolImageUrl(long toolID, string imageUrl)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageUrl, imageUrl));

    public async Task<IList<Tool>> GetTop3Tools()
        => await db.Tools.OrderByDescending(o => o.Created).Take(3).ToListAsync();

    public async Task ChangeCategory(long toolID, long categoryID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));
}