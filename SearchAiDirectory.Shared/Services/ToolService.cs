namespace SearchAiDirectory.Shared.Services;

public interface IToolService
{
    Task<bool> CategoryExists(string categoryName);
    Task<long> GetCategoryIDByName(string categoryName);
    Task<IList<ToolCategory>> GetCategoriesWithTools();
    Task<ToolCategory> GetCategoryBySlug(string slug);
    Task<IList<Tool>> GetCategoryTools(long categoryID);
    Task<long> AddCategory(ToolCategory newCategory);
    Task UpdateCategory(ToolCategory toolCategory);
    Task<IList<Tool>> GetAllTools();
    Task<bool> ToolExists(string name);
    Task<Tool> GetToolByID(long toolID);
    Task<Tool> GetToolByName(string toolName);
    Task<Tool> GetToolBySlug(string toolSlug);
    Task<long> AddTool(Tool newTool);
    Task UpdateTool(Tool updatedTool);
    Task<IList<ToolCategory>> GetAllCategories();
    Task<IList<ToolCategory>> GetActiveCategories();
    Task ChangeToolCategory(long toolID, long categoryID);
    Task UpdateToolImageUrl(long toolID, string imageUrl);
    Task DeleteTool(long toolID);
    Task<IList<Tool>> GetTop3Tools();
}

public class ToolService(ApplicationDataContext db) : IToolService
{
    public async Task<bool> CategoryExists(string categoryName)
        => await db.ToolCategories.AnyAsync(a => a.Name.ToLower() == categoryName.ToLower());

    public async Task<long> GetCategoryIDByName(string categoryName)
        => await db.ToolCategories.Where(w => w.Name.ToLower() == categoryName.ToLower()).Select(s => s.ID).SingleOrDefaultAsync();
    public async Task<IList<ToolCategory>> GetAllCategories()
        => await db.ToolCategories.Include(i => i.Tools).OrderBy(o => o.Name).ToListAsync();

    public async Task<IList<ToolCategory>> GetActiveCategories()
        => await db.ToolCategories.Include(i => i.Tools).Where(w => w.Tools.Count > 0).OrderBy(o => o.Name).ToListAsync();
    public async Task<IList<ToolCategory>> GetCategoriesWithTools()
        => await db.ToolCategories.Include(i => i.Tools).ToListAsync();

    public async Task<ToolCategory> GetCategoryBySlug(string slug)
        => await db.ToolCategories.Include(i => i.Tools).Where(w => w.Slug == slug).SingleOrDefaultAsync();
    public async Task<long> AddCategory(ToolCategory newCategory)
    {
        await db.ToolCategories.AddAsync(newCategory);
        await db.SaveChangesAsync();
        return newCategory.ID;
    }

    public async Task UpdateCategory(ToolCategory toolCategory)
    {
        db.Update(toolCategory);
        await db.SaveChangesAsync();
    }

    public async Task<IList<Tool>> GetCategoryTools(long categoryID)
        => await db.Tools.Include(i => i.Category).Where(w => w.CategoryID == categoryID).ToListAsync();

    public async Task<IList<Tool>> GetAllTools()
        => await db.Tools.ToListAsync();

    public async Task<bool> ToolExists(string name)
        => await db.Tools.AnyAsync(a => a.Name.ToLower() == name.ToLower());

    public async Task<Tool> GetToolByID(long toolID)
        => await db.Tools.Include(i => i.Category).Where(w => w.ID == toolID).SingleOrDefaultAsync();

    public async Task<Tool> GetToolByName(string toolName)
        => await db.Tools.Include(i => i.Category).Where(w => w.Name.ToLower() == toolName.ToLower()).SingleOrDefaultAsync();

    public async Task<Tool> GetToolBySlug(string toolSlug)
        => await db.Tools.Include(i => i.Category).Where(db => db.Slug == toolSlug).SingleOrDefaultAsync();

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

    public async Task ChangeToolCategory(long toolID, long categoryID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));

    public async Task UpdateToolImageUrl(long toolID, string imageUrl)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageUrl, imageUrl));

    public async Task DeleteTool(long toolID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteDeleteAsync();

    public async Task<IList<Tool>> GetTop3Tools()
        => await db.Tools.OrderByDescending(o => o.Created).Take(3).ToListAsync();
}
