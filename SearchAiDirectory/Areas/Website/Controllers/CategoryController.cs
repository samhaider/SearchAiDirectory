namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
public class CategoryController(ICategoryService categoryService) : Controller
{
    [HttpGet("/categories")]
    public async Task<IActionResult> Index()
    {
        var categories = await categoryService.GetActiveCategories();
        return View(categories);
    }

    [HttpGet("/tool/category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await categoryService.GetCategoryBySlug(slug);
        return View(category);
    }
}
