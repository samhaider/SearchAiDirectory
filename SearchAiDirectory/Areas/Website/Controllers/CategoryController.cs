namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
public class CategoryController(IToolService toolService) : Controller
{
    [HttpGet("/categories")]
    public async Task<IActionResult> Index()
    {
        var categories = await toolService.GetActiveCategories();
        return View(categories);
    }

    [HttpGet("/tool/category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await toolService.GetCategoryBySlug(slug);
        return View(category);
    }
}
