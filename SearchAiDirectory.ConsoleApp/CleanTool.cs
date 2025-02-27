using HtmlAgilityPack;
using SearchAiDirectory.Shared.Utilities;

namespace SearchAiDirectory.ConsoleApp;

public static class CleanTools
{
    public static async Task Clean()
    {
        var services = BgUtil.GetServices();
        using var scope = services.CreateScope();
        var toolService = scope.ServiceProvider.GetRequiredService<IToolService>();
        //var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();

        //var allRows = LoadData.LoadCsv();
        var tools = await toolService.GetAllTools();
        //var allNews = await newsService.GetAllNews();

        //foreach (var tool in tools)
        //{
        //    Console.WriteLine($"Creating Embedding for: {tool.Name}");
        //    await toolService.CreateEmbeddingRecord(tool.ID);
        //}

        //foreach (var news in allNews)
        //{
        //    Console.WriteLine($"Creating Embedding for: {news.Title}");
        //    await newsService.CreateNewsEmbedding(news.ID);
        //}

        //var categoryTools = tools.Where(w => !w.CategoryID.HasValue).ToList();
        //foreach (var tool in categoryTools)
        //{
        //    var allCategories = await toolService.GetAllCategories();
        //    var categoryList = string.Join("\n", allCategories.Select(s => $"ID:{s.ID} |Name:{s.Name}").ToArray());
        //    var prompt = $"Select or create a category for the ai tool with the following details:" +
        //        $"\nName: '{tool.Name}'" +
        //        $"\nDescription: {tool.Description}" +
        //        $"\n\nFollowing are the current available categories:" +
        //        $"{categoryList}" +
        //        $"\n\nResponse ONLY AND ONLY with the category name, without any additional context.";
        //    var categoryAgent = new AgentBase("Categorize Ai Tool", .8, SystemPrompts.Categorizing);
        //    var categoryResponse = await categoryAgent.DirectOpenAiResponse(prompt);
        //    string categoryName = categoryResponse
        //        .Replace("\'", "")
        //        .Replace("\"", "")
        //        .Replace(".", "")
        //        .Replace("`", "")
        //        .Trim().Normalize();

        //    if (categoryName.Contains("categor", StringComparison.OrdinalIgnoreCase)) continue;

        //    long categoryID = 0;
        //    if (!await toolService.CategoryExists(categoryName))
        //    {
        //        string categorgyMetaDescription = await categoryAgent.DirectOpenAiResponse("Create a meta description for this category without any context. the meta description should be less than 140 characters.");
        //        string categorgyMetaKeywords = await categoryAgent.DirectOpenAiResponse("Create a comma delimited meta keywords for this category without any context. the meta keywords should be less than 140 characters.");

        //        categoryID = await toolService.AddCategory(
        //            new Category
        //            {
        //                Name = categoryName,
        //                MetaDescription = categorgyMetaDescription.Trim().Normalize(),
        //                MetaKeywords = categorgyMetaKeywords.Trim().Normalize()
        //            });
        //    }
        //    else
        //    {
        //        categoryID = await toolService.GetCategoryIDByName(categoryName);
        //    }

        //    if (categoryID != 0) await toolService.ChangeCategory(tool.ID, categoryID);
        //}

        //var websiteTools = tools.Where(w => !w.Website.Contains("?ref=")).ToList();
        //foreach (var tool in websiteTools)
        //{
        //    Console.WriteLine($"Updating Website for: {tool.Name}");
        //    var orgWebsite = allRows.Where(w => w.Name == tool.Name).Select(s => s.DetailPage).FirstOrDefault();

        //    var link = await ExtractLink(orgWebsite);
        //    if (!string.IsNullOrEmpty(link))
        //    {
        //        tool.Website = await WebScraper.GetFinalUrl(link);
        //        if (IsValidUrl(tool.Website))
        //        {
        //            tool.Website = GetBaseUrl(tool.Website);

        //            if (tool.Website.EndsWith("/"))
        //                tool.Website += "?ref=searchaidirectory.com";
        //            else
        //                tool.Website += "/?ref=searchaidirectory.com";
        //            await toolService.UpdateTool(tool);
        //        }
        //    }
        //}


        tools = tools.Where(w => string.IsNullOrEmpty(w.ImageUrl)).ToList();
        foreach (var tool in tools)
        {
            Console.WriteLine($"Extracting Image for: " + tool.Name);

            if (!string.IsNullOrEmpty(tool.ImageUrl) && IsValidUrl(tool.ImageUrl)) continue;
            if (string.IsNullOrEmpty(tool.Website) || !IsValidUrl(tool.Website)) continue;

            var imageByteArray = await WebsiteScrapper.GetWebsiteScreenshot(tool.Website);
            if (imageByteArray is null) continue;

            var azImageUrl = await AzureStorageService.UploadBlobFiles(
                    BlobContainers.tools,
                    $"/{tool.ID}/{tool.Name.ToLower().Replace(" ", "")}.png",
                    imageByteArray);
            await toolService.UpdateToolImageUrl(tool.ID, azImageUrl);
        }

        //await toolService.UpdateTool(dbTool);
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    public static async Task<string> ExtractLink(string siteUrl)
    {
        using var httpClient = new HttpClient();
        try
        {
            var htmlContent = await httpClient.GetStringAsync(siteUrl);

            // Load the HTML content into an HtmlDocument
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            // Find the div with class "div-block-6 vertical-flex"
            var divNode = document.DocumentNode.SelectSingleNode("//div[@class='div-block-6 vertical-flex']");

            if (divNode != null)
            {
                // Find the first <a> tag with the specific class
                var linkNode = divNode.SelectSingleNode(".//a[@class='link-block-2 w-inline-block']");

                // Extract the href attribute
                string link = linkNode?.GetAttributeValue("href", null);

                return link;
            }
            else
            {
                return string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }
    public static async Task<string> ExtractImageUrl(string siteUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            var htmlContent = await httpClient.GetStringAsync(siteUrl);

            // Load the HTML content into an HtmlDocument
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            // Find the div with the specific ID
            var divNode = document.DocumentNode.SelectSingleNode("//div[@id='w-node-_0d11088c-1bcb-2225-b918-232ca35a6afe-799ae876']");

            if (divNode != null)
            {
                // Find the <img> tag within the div
                var imgNode = divNode.SelectSingleNode(".//img[@class='image-3']");

                // Extract the src attribute
                string imageUrl = imgNode?.GetAttributeValue("src", null);

                return imageUrl;
            }
            else
            {
                return string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }
    private static string GetBaseUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return $"{uriResult.Scheme}://{uriResult.Host}";
        }
        return string.Empty;
    }
}
