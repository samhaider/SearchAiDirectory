namespace SearchAiDirectory.Shared.Services;

public interface IToolService
{
    Task<bool> CategoryExists(string categoryName);
    Task<long> GetCategoryIDByName(string categoryName);
    Task<IList<ToolCategory>> GetCategoriesWithTools();
    Task<IList<Tool>> GetCategoryTools(long categoryID);
    Task<long> AddCategory(string categoryName);
    Task UpdateCategory(long categoryID, string categoryName);
    Task<IList<Tool>> GetAllTools();
    Task<bool> ToolExists(string name);
    Task<Tool> GetToolByID(long toolID);
    Task AddTool(Tool newTool);
    Task UpdateTool(Tool updatedTool);
    Task<IList<ToolCategory>> GetAllCategories();
    Task ChangeToolCategory(long toolID, short categoryID);
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
        => await db.ToolCategories.OrderBy(o => o.Name).ToListAsync();

    public async Task<IList<ToolCategory>> GetCategoriesWithTools()
        => await db.ToolCategories.Include(i => i.Tools).ToListAsync();

    public async Task<long> AddCategory(string categoryName)
    {
        var toolCategory = new ToolCategory() { Name = categoryName };
        await db.ToolCategories.AddAsync(toolCategory);
        await db.SaveChangesAsync();
        return toolCategory.ID;
    }

    public async Task UpdateCategory(long categoryID, string categoryName)
    {
        var category = await db.ToolCategories.FindAsync(categoryID);
        category.Name = categoryName;

        db.Update(category);
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

    public async Task AddTool(Tool newTool)
    {
        await db.Tools.AddAsync(newTool);
        await db.SaveChangesAsync();
    }

    public async Task UpdateTool(Tool updateTool)
    {
        db.Tools.Update(updateTool);
        await db.SaveChangesAsync();
    }

    public async Task ChangeToolCategory(long toolID, short categoryID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));

    public async Task DeleteTool(long toolID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteDeleteAsync();

    public async Task<IList<Tool>> GetTop3Tools()
        => await db.Tools.OrderByDescending(o => o.Created).Take(3).ToListAsync();
}
