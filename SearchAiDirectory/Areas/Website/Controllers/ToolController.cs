﻿namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class ToolController(IToolService toolService, ICategoryService categoryService, ILikeService likeService, ICommentService commentService, IHttpContextAccessor httpContextAccessor) : Controller
{
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext;

    [HttpGet("/categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await categoryService.GetActiveCategories();
        return View(categories);
    }

    [HttpGet("/category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await categoryService.GetCategoryBySlug(slug);
        return View(category);
    }

    [HttpGet("/tools")]
    public async Task<IActionResult> Tools()
    {
        var tools = await toolService.GetAllTools();
        return View(tools);
    }

    [HttpGet] public IActionResult ToolNotFound() => View();

    [HttpGet("/tool/{slug}")]
    public async Task<IActionResult> Tool(string slug)
    {
        var tool = await toolService.GetToolBySlug(slug);
        if (tool is null) return RedirectToAction("ToolNotFound");

        var model = new ToolPageModel() { Tool = tool };
        if (tool.Embedding is not null)
            model.RelatedTools = await toolService.Get3RelatedTools(tool.ID, tool.Embedding.EmbeddingCode);

        // Check if the current user has liked this tool
        if (httpContext.User.Identity.IsAuthenticated)
        {
            var userId = long.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            model.IsLikedByCurrentUser = await likeService.HasUserLikedTool(userId, model.Tool.ID);
        }

        return View(model);
    }

    [HttpPost("/tool/search")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(string query)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(query)) return Redirect("/");

        var queryEmbedding = await OpenAiService.GetEmbedding(query);
        var tools = await toolService.EmbeddingSearchTools(queryEmbedding, 9);
        return View("Tools", tools);
    }

    [HttpPost("/tool/togglelike/{toolId}")]
    public async Task<IActionResult> ToggleLike(long toolId)
    {
        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { success = false, message = "User must be logged in to like tools" });
        }

        try
        {
            var userId = long.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var (IsLiked, LikeCount) = await likeService.ToggleLike(userId, toolId);

            return Json(new
            {
                success = true,
                isLiked = IsLiked,
                likeCount = LikeCount
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("/tool/comment/{toolId}")]
    public async Task<IActionResult> Comment(long toolId, string content)
    {
        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { success = false, message = "User must be logged in to comment" });
        }

        try
        {
            var userId = long.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var comment = new Comment
            {
                UserID = userId,
                ToolID = toolId,
                Content = content,
                Approve = true // Auto-approve comments for now
            };

            await commentService.AddComment(comment);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
