namespace SearchAiDirectory.Areas.Website.Models;

public class ToolPageModel
{
    public Tool Tool { get; set; }
    public IList<Tool> RelatedTools { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}
