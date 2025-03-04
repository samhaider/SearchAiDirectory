﻿namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[Route("[action]")]
[OutputCache(PolicyName = "GlobalCachePolicy")]
public class SharedController(IToolService toolService, ICategoryService categoryService) : Controller
{
    [HttpGet] public IActionResult Error() => View();
    [HttpGet] public IActionResult NoJs() => View();

    //Sitemap
    [HttpGet]
    [Route("/sitemap")]
    [Route("/sitemap.xml")]
    public async Task<IActionResult> SiteMap()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XElement root = new(xmlns + "urlset");

        var pages = new List<KeyValuePair<string, DateTime>>
        {
            new("https://searchaidirectory.com/", new DateTime(2024,06,21,9,3,00)),
            new("https://searchaidirectory.com/tools", new DateTime(2024,09,18,16,12,15)),
            new("https://searchaidirectory.com/categories", new DateTime(2024,09,18,16,12,15)),
            new("https://searchaidirectory.com/news", new DateTime(2024,10,01,08,3,00)),
            new("https://searchaidirectory.com/contact", new DateTime(2024,10,28,08,3,00)),
            new("https://searchaidirectory.com/newsletter", new DateTime(2024,07,01,12,3,00)),
            new("https://searchaidirectory.com/terms/privacy", new DateTime(2024,06,21,10,24,26)),
            new("https://searchaidirectory.com/terms/cookie", new DateTime(2024,06,21,10,24,26)),
            new("https://searchaidirectory.com/terms/disclaimer", new DateTime(2024,06,21,10,24,26))
        };
        foreach (var page in pages)
        {
            XElement urlElement =
                new(xmlns + "url",
                new XElement(xmlns + "loc", new Uri(page.Key).AbsoluteUri),
                new XElement(xmlns + "lastmod", page.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(xmlns + "priority", "1.0"));
            root.Add(urlElement);
        }

        var toolsTask = toolService.GetAllTools();
        var categoriesTask = categoryService.GetActiveCategories();
        await Task.WhenAll(toolsTask, categoriesTask);

        foreach (var tool in toolsTask.Result)
        {
            XElement urlElement =
                new(xmlns + "url",
                new XElement(xmlns + "loc", new Uri("https://searchaidirectory.com/tool/" + tool.Slug).AbsoluteUri),
                new XElement(xmlns + "lastmod", tool.Created.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(xmlns + "priority", "0.9"));
            root.Add(urlElement);
        }

        foreach (var category in categoriesTask.Result)
        {
            XElement urlElement =
            new(xmlns + "url",
                new XElement(xmlns + "loc", new Uri("https://searchaidirectory.com/category/" + category.Slug).AbsoluteUri),
                new XElement(xmlns + "lastmod", category.Created.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(xmlns + "priority", "0.9"));
            root.Add(urlElement);
        }

        XDocument document = new(root);
        return Content(document.ToString(), "text/xml");
    }

    //Robots File
    [HttpGet("/robots.txt")]
    public ContentResult RobotsTxt()
    {
        var sb = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Disallow: /css/")
            .AppendLine("Disallow: /fonts/")
            .AppendLine("Disallow: /img/")
            .AppendLine("Disallow: /js/")
            .AppendLine("Disallow: /cdn-cgi")
            .AppendLine("Disallow: /?blackhole")
            .Append("sitemap: ")
            .Append(Request.Scheme)
            .Append("://")
            .Append(Request.Host)
            .AppendLine("/sitemap");

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}