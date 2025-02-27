namespace SearchAiDirectory.Areas.Admin.Controllers;

[Authorize]
[Area("Admin")]
[Route("[area]/[controller]/[action]")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class NewsAdminController(INewsService newsService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var allNews = await newsService.GetAllNews();
        return View(allNews);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> Edit(long ID)
    {
        var news = await newsService.GetNewsByID(ID);
        return View(news);
    }
}
