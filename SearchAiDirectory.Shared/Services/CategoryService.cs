namespace SearchAiDirectory.Shared.Services;

public interface ICategoryService
{
    Task<long> AddCategory(Category newCategory);
    Task<bool> CategoryExists(string categoryName);
    Task<IList<Category>> GetActiveCategories();
    Task<IList<Category>> GetAllCategories();
    Task<IList<Category>> GetCategoriesWithTools();
    Task<Category> GetCategoryBySlug(string slug);
    Task<long> GetCategoryIDByName(string categoryName);
    Task<IList<Tool>> GetCategoryTools(long categoryID);
    Task UpdateCategory(Category updatedCategory);
}

public class CategoryService(ApplicationDataContext db) : ICategoryService
{
    public async Task<bool> CategoryExists(string categoryName)
        => await db.Categories.AnyAsync(a => a.Name.ToLower() == categoryName.ToLower());

    public async Task<long> GetCategoryIDByName(string categoryName)
        => await db.Categories.Where(w => w.Name.ToLower() == categoryName.ToLower()).Select(s => s.ID).SingleOrDefaultAsync();

    public async Task<IList<Category>> GetAllCategories()
        => await db.Categories.Include(i => i.Tools).OrderBy(o => o.Name).ToListAsync();

    public async Task<IList<Category>> GetActiveCategories()
        => await db.Categories.Include(i => i.Tools).Where(w => w.Tools.Count > 0).OrderBy(o => o.Name).ToListAsync();

    public async Task<IList<Category>> GetCategoriesWithTools()
        => await db.Categories.Include(i => i.Tools).ToListAsync();

    public async Task<Category> GetCategoryBySlug(string slug)
        => await db.Categories.Include(i => i.Tools).Where(w => w.Slug == slug).SingleOrDefaultAsync();

    public async Task<long> AddCategory(Category newCategory)
    {
        newCategory.Created = DateTime.UtcNow;
        newCategory.Slug = RegexHelper.TextToSlug(newCategory.Name);
        await db.Categories.AddAsync(newCategory);
        await db.SaveChangesAsync();
        return newCategory.ID;
    }

    public async Task UpdateCategory(Category updatedCategory)
    {
        updatedCategory.Created = DateTime.UtcNow;
        updatedCategory.Slug = RegexHelper.TextToSlug(updatedCategory.Name);
        db.Update(updatedCategory);
        await db.SaveChangesAsync();
    }

    public async Task<IList<Tool>> GetCategoryTools(long categoryID)
        => await db.Tools.Include(i => i.Category).Where(w => w.CategoryID == categoryID).ToListAsync();

}
