using HtmlAgilityPack;

namespace SearchAiDirectory.ConsoleApp;

public static class CleanTools
{
    public static async Task Clean()
    {
        var services = BgUtil.GetServices();
        using var scope = services.CreateScope();
        var toolService = scope.ServiceProvider.GetRequiredService<IToolService>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        var allRows = LoadData.LoadCsv();
        var tools = await toolService.GetAllTools();

        foreach (var tool in tools)
            await embeddingService.CreateEmbeddingRecord(tool.ID);


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

        //var validImageExtensions = new List<string> { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".webp", ".tiff", ".ico", ".heic", ".avif" };
        //var imageTools = tools.Where(w =>
        //    !w.ImageUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) &&
        //    !w.ImageUrl.EndsWith(".avif", StringComparison.OrdinalIgnoreCase)
        //).ToList();
        //foreach (var tool in imageTools)
        //{
        //    var orgWebsite = allRows.Where(w => w.Name == tool.Name).Select(s => s.DetailPage).FirstOrDefault();

        //    var imageUrl = await ExtractImageUrl(orgWebsite);
        //    if (!string.IsNullOrEmpty(imageUrl) && IsValidUrl(imageUrl))
        //    {
        //        using var httpClient = new HttpClient();
        //        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

        //        var imageExtension = Path.GetExtension(imageUrl);
        //        var azImageUrl = await AzureStorageService.UploadBlobFiles(
        //            BlobContainers.tools,
        //            $"/{tool.ID}/{tool.Name.ToLower().Replace(" ", "")}{imageExtension}",
        //            imageBytes);
        //        await toolService.UpdateToolImageUrl(tool.ID, azImageUrl);
        //    }
        //}

        //var allCategories = await toolService.GetAllCategories();
        //var categoryList = string.Join("\n", allCategories.Select(s => $"ID:{s.ID} |Name:{s.Name}").ToArray());

        //var prompt = $"Create a category for the ai tool with the following details:\nName: {tool.Name}\nDescription: {tool.Description}" +
        //    $"\nFollowing are the current Categories:" +
        //    $"\n{categoryList}";
        //var categoryAgent = new AgentBase("Categorize Ai Tool", .6, SystemPrompts.Categorizing);
        //var categoryResponse = await categoryAgent.DirectOpenAiResponse(prompt);
        //var categoryName = categoryResponse.Replace("Category:", "").Replace("\'", "").Replace("\"", "").Replace(".", "").Replace("`", "").Trim();

        //if (!(dbTool.Category.Name == categoryResponse))
        //{
        //    if (await toolService.CategoryExists(categoryResponse))
        //    {
        //        var categoryID = await toolService.GetCategoryIDByName(categoryResponse);
        //        dbTool.CategoryID = categoryID;
        //    }
        //    else
        //    {
        //        string categorgyMetaDescription = await categoryAgent.DirectOpenAiResponse("Create a meta description for this category without any context. the meta description should be less than 140 characters.");
        //        string categorgyMetaKeywords = await categoryAgent.DirectOpenAiResponse("Create a comma delimited meta keywords for this category without any context. the meta keywords should be less than 140 characters.");
        //        dbTool.CategoryID = await toolService.AddCategory(
        //            new ToolCategory
        //            {
        //                Name = categoryName,
        //                Slug = RegexHelper.TextToSlug(categoryName),
        //                MetaDescription = categorgyMetaDescription.Trim().Normalize(),
        //                MetaKeywords = categorgyMetaKeywords.Trim().Normalize()
        //            });
        //    }
        //}

        //await toolService.UpdateTool(dbTool);
        //await embeddingService.CreateEmbeddingRecord(dbTool.ID);
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
            HtmlDocument document = new HtmlDocument();
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
