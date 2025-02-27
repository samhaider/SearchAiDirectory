namespace SearchAiDirectory.Areas.Admin.Controllers;

[Authorize]
[Area("Admin")]
[Route("[area]/[controller]/[action]")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class ToolAdminController(IToolService toolService) : Controller
{

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var tools = await toolService.GetAllTools();
        return View(tools);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> Edit(long ID)
    {
        var tool = await toolService.GetToolByID(ID);
        return View(tool);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> Delete(long ID)
    {
        await toolService.DeleteTool(ID);
        return RedirectToAction("List");
    }
}
