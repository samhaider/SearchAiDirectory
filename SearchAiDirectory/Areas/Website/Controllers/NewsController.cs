namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class NewsController(INewsService newsService) : Controller
{
    [HttpGet("/news")]
    public async Task<IActionResult> List()
    {
        var news = await newsService.GetAllNews();
        return View(news);
    }

    [HttpGet("/news/{slug}")]
    public async Task<IActionResult> News(string slug)
    {
        var news = await newsService.GetNewsBySlug(slug);

        var model = new NewsPageModel() { News = news };
        if (news.Embedding is not null)
            model.RelatedNews = await newsService.Get3RelatedNews(news.ID, news.Embedding.EmbeddingCode);

        return View(model);
    }
}
