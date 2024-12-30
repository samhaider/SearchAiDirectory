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

    [HttpGet] public IActionResult About() => View();

    [HttpGet] public IActionResult Contact() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(string name, string email, string message)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Message = "Please fill out the form properly before sending the message.";
            return View();
        }

        await SendGridService.SendFormSubmissionEmail(name, email, message);
        ViewBag.Message = "Thank you for your message! We will get back to you as soon as possible.";
        return View();
    }
}