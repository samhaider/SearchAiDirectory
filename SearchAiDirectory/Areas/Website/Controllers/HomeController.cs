using SearchAiDirectory.Areas.Website.Models;

namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[Route("[action]")]
public class HomeController(IToolService toolService) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var model = new HomePageModel()
        {
            Categories = await toolService.GetAllCategories(),
            Top3Tools = await toolService.GetTop3Tools()
        };
        return View(model);
    }
}