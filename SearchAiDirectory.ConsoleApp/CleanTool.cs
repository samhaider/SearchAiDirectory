using SearchAiDirectory.Shared.Utilities;

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
        foreach (var tool in allRows)
        {
            if (string.IsNullOrEmpty(tool.Name) || string.IsNullOrEmpty(tool.LinkPage)) continue;
            if (!await toolService.ToolExists(tool.Name.Trim().Normalize())) continue;

            var dbTool = await toolService.GetToolByName(tool.Name.Trim().Normalize());
            //if (dbTool is null || dbTool.ID < 403) continue;

            Console.WriteLine("Cleaning Tool Record: " + tool.Name);
            if (string.IsNullOrEmpty(dbTool.ImageUrl) && IsValidUrl(tool.ImageUrl))
            {
                var imageExtension = Path.GetExtension(tool.ImageUrl);
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(tool.ImageUrl);
                var imageUrl = await AzureStorageService.UploadBlobFiles(
                    BlobContainers.tools,
                    $"/{dbTool.ID}/{dbTool.Name.ToLower().Replace(" ", "")}{imageExtension}",
                    imageBytes);

                await toolService.UpdateToolImageUrl(dbTool.ID, imageUrl);
            }

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
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
