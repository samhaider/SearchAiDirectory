namespace SearchAiDirectory.Controllers;

public class ToolController(IToolService toolService) : Controller
{
    [HttpGet("Listings")]
    public async Task<IActionResult> List()
    {
        var tools = await toolService.GetAllTools();
        return View(tools);
    }

    [HttpPost]
    public async Task<IActionResult> Search(string query)
    {
        var tools = await toolService.SearchTools(query);
        return View("List", tools);
    }

    [HttpGet("/Listing/Tool/{toolID}")]
    public async Task<IActionResult> Tool(long toolID)
    {
        var tool = await toolService.GetToolByID(toolID);
        return View(tool);
    }

    [HttpGet("/Listing/Information/{hash}")]
    public async Task<IActionResult> Information(string hash)
    {
        if(string.IsNullOrEmpty(hash)) return RedirectToAction("List");

        var tool = await toolService.GetToolFromHash(hash);
        
        if (tool is null) return RedirectToAction("List");

        ViewBag.Categories = await toolService.GetAllCategories();

        return View(tool);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInformation(Tool updateTool)
    {
        await toolService.UpdateTool(updateTool);
        return Redirect($"/Listing/Tool/{updateTool.ID}");
    }

    [HttpPost]
    public async Task UpdateCategory(long toolID, short categoryID)
        => await toolService.ChangeToolCategory(toolID, categoryID);

    [HttpGet]
    public async Task<IActionResult> DeleteTool(long toolID)
    {
        await toolService.DeleteTool(toolID);
        return RedirectToAction("List");
    }
}
