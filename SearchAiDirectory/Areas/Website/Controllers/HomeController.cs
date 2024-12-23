namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[Route("[action]")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class HomeController(IToolService toolService, ICategoryService categoryService) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var categoriesTask = categoryService.GetAllCategories();
        var top3ToolsTask = toolService.GetTop3Tools();
        await Task.WhenAll(categoriesTask, top3ToolsTask);

        var model = new HomePageModel() { Categories = categoriesTask.Result, Top3Tools = top3ToolsTask.Result };
        return View(model);
    }
}