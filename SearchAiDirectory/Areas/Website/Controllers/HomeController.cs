namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[Route("[action]")]
public class HomeController(IToolService toolService, ICategoryService categoryService) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var model = new HomePageModel()
        {
            Categories = await categoryService.GetAllCategories(),
            Top3Tools = await toolService.GetTop3Tools()
        };
        return View(model);
    }
}