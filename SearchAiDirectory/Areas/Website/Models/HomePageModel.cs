namespace SearchAiDirectory.Areas.Website.Models;

public class HomePageModel
{
    public IList<Category> Categories { get; set; }
    public IList<Tool> Top3Tools { get; set; }
}
