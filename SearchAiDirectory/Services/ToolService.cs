namespace SearchAiDirectory.Services;

public interface IToolService
{
    Task<IList<Category>> GetToolCategories();
    Task<IList<Tool>> GetCategoryTools(long categoryID);
    Task AddCategory(string categoryName);
    Task UpdateCategory(long categoryID, string categoryName);
    Task<IList<Tool>> GetAllTools();
    Task<IList<Tool>> SearchTools(string query);
    Task<Tool> GetToolByID(long toolID);
    Task AddTool(Tool newTool);
    Task<string> GetHashForTool(long toolID);
    Task<Tool> GetToolFromHash(string hash);
    Task UpdateTool(Tool updatedTool);
    Task<IList<Category>> GetAllCategories();
    Task ChangeToolCategory(long toolID, short categoryID);
    Task DeleteTool(long toolID);
}

public class ToolService(ApplicationDataContext db) : IToolService
{
    public async Task<IList<Category>> GetToolCategories()
        => await db.Categories.Include(i => i.Tools).ToListAsync();

    public async Task<IList<Tool>> GetCategoryTools(long categoryID)
        => await db.Tools.Include(i => i.Category).Where(w => w.CategoryID == categoryID).ToListAsync();

    public async Task AddCategory(string categoryName)
    {
        await db.Categories.AddAsync(new Category() { Name = categoryName });
        await db.SaveChangesAsync();
    }

    public async Task UpdateCategory(long categoryID, string categoryName)
    {
        var category = await db.Categories.FindAsync(categoryID);
        category.Name = categoryName;

        db.Update(category);
        await db.SaveChangesAsync();
    }

    public async Task<IList<Tool>> GetAllTools()
        => await db.Tools.ToListAsync();

    public async Task<IList<Tool>> SearchTools(string query)
    {
        return await db.Tools
            .Include(i => i.Category)
            .Where(w => w.Name.ToLower().Contains(query.ToLower()) || w.Description.ToLower().Contains(query.ToLower()) || w.Category.Name.ToLower().Contains(query.ToLower()))
            .ToListAsync();
    }

    public async Task<Tool> GetToolByID(long toolID)
        => await db.Tools.Include(i => i.Category).Where(w => w.ID == toolID).SingleOrDefaultAsync();

    public async Task AddTool(Tool newTool)
    {
        await db.Tools.AddAsync(newTool);
        await db.SaveChangesAsync();
    }

    public async Task<string> GetHashForTool(long toolID)
    {
        var tool = await db.Tools.FindAsync(toolID);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tool.ID.ToString()));
    }

    public async Task<Tool> GetToolFromHash(string hash)
    {
        try
        {
            var toolID = Convert.ToInt64(Encoding.UTF8.GetString(Convert.FromBase64String(hash)));
            return await GetToolByID(toolID);
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateTool(Tool updateTool)
    {
        db.Tools.Update(updateTool);
        await db.SaveChangesAsync();
    }

    public async Task<IList<Category>> GetAllCategories()
        => await db.Categories.OrderBy(o => o.Name).ToListAsync();

    public async Task ChangeToolCategory(long toolID, short categoryID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));

    public async Task DeleteTool(long toolID)
        => await db.Tools.Where(w => w.ID == toolID).ExecuteDeleteAsync();
}
