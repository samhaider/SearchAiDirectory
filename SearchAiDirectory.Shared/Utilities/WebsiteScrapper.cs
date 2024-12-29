using HtmlAgilityPack;
using PuppeteerSharp;

namespace SearchAiDirectory.Shared.Utilities;

public static class WebsiteScrapper
{
    private static async Task<HtmlDocument> LoadWebsite(string url)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            string htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            return htmlDocument;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string> GetWebsiteTextContent(string url)
    {
        var htmlDocument = await LoadWebsite(url);
        if (htmlDocument is null) return null;

        // Assuming the main content is within <article> tags
        var articleNode = htmlDocument.DocumentNode.SelectSingleNode("//article");
        string content = articleNode is not null ? articleNode.InnerText : string.Empty;

        // Fallback to body content if article tag is not found
        if (string.IsNullOrEmpty(content))
        {
            var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
            content = bodyNode is not null ? bodyNode.InnerText : string.Empty;
        }

        return RegexHelper.HtmlToCleanText(content);
    }

    public static async Task<string> GetWebsiteHtmlContent(string url)
    {
        var htmlDocument = await LoadWebsite(url);
        if (htmlDocument is null) return null;

        // Assuming the main content is within <article> tags
        var articleNode = htmlDocument.DocumentNode.SelectSingleNode("//article");
        string content = articleNode is not null ? articleNode.InnerHtml : string.Empty;

        // Fallback to body content if article tag is not found
        if (string.IsNullOrEmpty(content))
        {

            var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
            content = bodyNode is not null ? bodyNode.InnerHtml : string.Empty;
        }

        return RegexHelper.HtmlToCleanText(content);
    }

    public static async Task<string> ScrapeWebsiteWithInternalLinks(string url)
    {
        var htmlDocument = await LoadWebsite(url);
        if (htmlDocument is null) return null;

        var contentBuilder = new StringBuilder();

        // Get main content from the home page
        var mainContent = await GetWebsiteTextContent(url);
        contentBuilder.Append(mainContent);

        // Find all internal links
        var internalLinks = htmlDocument.DocumentNode.SelectNodes("//a[@href]")
            .Select(node => node.GetAttributeValue("href", string.Empty))
            .Where(href => href.StartsWith("/") || href.StartsWith(url))
            .Distinct()
            .ToList();

        // Scrape content from each internal link
        foreach (var link in internalLinks)
        {
            var fullLink = link.StartsWith("/") ? $"{url.TrimEnd('/')}{link}" : link;
            var linkContent = await GetWebsiteTextContent(fullLink);
            contentBuilder.Append(linkContent);
        }

        return contentBuilder.ToString();
    }

    public static async Task<byte[]> GetWebsiteScreenshot(string url)
    {
        var htmlDocument = await LoadWebsite(url);
        if (htmlDocument is null) return null;

        var browserFetcher = new BrowserFetcher(SupportedBrowser.Chromium);
        await browserFetcher.DownloadAsync();

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Browser = SupportedBrowser.Chromium,
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        });

        var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
        await page.GoToAsync(url);
        var screenshot = await page.ScreenshotDataAsync(new ScreenshotOptions { FullPage = false });
        await browser.CloseAsync();

        return screenshot;
    }
}
