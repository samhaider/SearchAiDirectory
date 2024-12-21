namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
public class ToolController(IToolService toolService, IEmbeddingService embeddingService) : Controller
{
    [HttpGet("/tools")]
    public async Task<IActionResult> List()
    {
        var tools = await toolService.GetAllTools();
        return View(tools);
    }

    [HttpGet("/tool/{slug}")]
    public async Task<IActionResult> Tool(string slug)
    {
        var tool = await toolService.GetToolBySlug(slug);
        return View(tool);
    }

    [HttpPost("/tool/search")]
    public async Task<IActionResult> Search(string query)
    {
        var queryEmbedding = await OpenAiService.GetEmbedding(query);
        var tools = await embeddingService.EmbeddingSearchTools(queryEmbedding, 9);
        return View("List", tools);
    }
}
