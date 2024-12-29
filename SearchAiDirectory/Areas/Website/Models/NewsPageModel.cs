namespace SearchAiDirectory.Areas.Website.Models;

public class NewsPageModel
{
    public News News { get; set; }
    public IList<News> RelatedNews { get; set; } = [];
}
