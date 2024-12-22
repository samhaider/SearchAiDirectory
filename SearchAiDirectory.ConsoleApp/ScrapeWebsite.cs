global using Microsoft.Extensions.DependencyInjection;
global using SearchAiDirectory.Shared.AiAgents;
global using SearchAiDirectory.Shared.Models;
global using SearchAiDirectory.Shared.Services;
global using SearchAiDirectory.Shared.ThirdPartyService;
global using System.Dynamic;
using SearchAiDirectory.Shared.Utilities;
using System.Text.RegularExpressions;
using static SearchAiDirectory.ConsoleApp.LoadData;

namespace SearchAiDirectory.ConsoleApp;

public static class ScrapeWebsite
{
    public static async Task Scrape()
    {
        var allRows = LoadCsv();

        var toolService = BgUtil.GetServices().GetRequiredService<IToolService>();
        var categoryService = BgUtil.GetServices().GetRequiredService<ICategoryService>();
        foreach (var tool in allRows)
        {
            var toolName = tool.Name.Trim().Normalize();
            if (string.IsNullOrEmpty(toolName) || string.IsNullOrEmpty(tool.LinkPage)) continue;
            if (await toolService.ToolExists(toolName)) continue;

            var researchAgent = new ResearchAgent();
            string websiteContent = await researchAgent.ResearchResponse(
                $"Tool Name: {toolName}" +
                $"\nTool Description: {tool.Description}" +
                $"\nTool Category: {tool.Category}");
            string websiteUrlResponse = await researchAgent.GetResponseAfterResearch("What is the website url of this app. No pre or post context is required, only return the website URL.");
            string websiteUrl = Regex.Match(websiteUrlResponse, @"http[s]?://\S+").Value.Trim();
            websiteUrl = Regex.Match(websiteUrlResponse, @"http[s]?://[^\s/$.?#].[^\s]*").Value.Trim();

            var websiteAgent = new AgentBase("WebsiteFAQ", 0.8, SystemPrompts.WebsiteFAQ);
            websiteAgent.SetupWebsiteContentForAi(websiteContent + "\n\n\nNO RESPONSE IS REQUIRED FOR THIS, AS THIS IS THE WEBSITE CONTENT, RESPOND THE NEXT INPUT.");
            var toolDescription = await websiteAgent.DirectOpenAiResponse("Give me a small description less than 450 character for this tool that would be helpful to the reader, and what problems it can solve. No pre context or post context is needed, just the description in plain text.");
            var metaDescription = await websiteAgent.DirectOpenAiResponse("Give me a meta description less than 140 character for this tool. no pre context or no post context is needed.");
            var metaKeywords = await websiteAgent.DirectOpenAiResponse("Give me a meta keywords less than 140 character in comma delimited format for this tool. no pre context or no post context is needed.");
            var pricingModel = await websiteAgent.DirectOpenAiResponse("Give me the pricing model of this tool in 1 to 3 words, like Free, Freemium, Paid only, Trial with Subscription, if unknown, response with Unknown. no pre context or no post context is needed.");

            var allCategories = await categoryService.GetAllCategories();
            var categoryList = string.Join("\n", allCategories.Select(s => $"ID:{s.ID} |Name:{s.Name}").ToArray());

            var prompt = $"Create a category for the ai tool with the following details:\nName: {tool.Name}\nDescription: {tool.Description}" +
                $"\nFollowing are the current Categories:" +
                $"\n{categoryList}";
            var categoryAgent = new AgentBase("Categorize Ai Tool", .6, SystemPrompts.Categorizing);
            var categoryResponse = await categoryAgent.DirectOpenAiResponse(prompt);
            string categoryName = categoryResponse.Replace("Category:", "").Replace("\'", "").Replace("\"", "").Replace(".", "").Replace("`", "").Trim().Normalize();
            string categorgyMetaDescription = await categoryAgent.DirectOpenAiResponse("Create a meta description for this category without any context. the meta description should be less than 140 characters.");
            string categorgyMetaKeywords = await categoryAgent.DirectOpenAiResponse("Create a comma delimited meta keywords for this category without any context. the meta keywords should be less than 140 characters.");
            long categoryID = !await categoryService.CategoryExists(categoryName)
                ? await categoryService.AddCategory(
                    new Category
                    {
                        Name = categoryName,
                        MetaDescription = categorgyMetaDescription.Trim().Normalize(),
                        MetaKeywords = categorgyMetaKeywords.Trim().Normalize()
                    })
                : await categoryService.GetCategoryIDByName(categoryName);

            var toolID = await toolService.AddTool(new Tool
            {
                CategoryID = categoryID,
                Name = toolName,
                Slug = RegexHelper.TextToSlug(toolName),
                Website = websiteUrl.Trim().Normalize(),
                WebsiteContent = websiteContent.Trim().Normalize(),
                Description = toolDescription.Trim().Normalize(),
                MetaDescription = metaDescription.Trim().Normalize(),
                MetaKeywords = metaKeywords.Trim().Normalize(),
                PriceModel = pricingModel.Replace(".", "").Trim().Normalize(),
                IsConfirmed = false,
                Created = DateTime.UtcNow,
            });

            if (!string.IsNullOrEmpty(tool.ImageUrl))
            {
                var imageExtension = Path.GetExtension(tool.ImageUrl);
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(tool.ImageUrl);
                var imageUrl = await AzureStorageService.UploadBlobFiles(
                    BlobContainers.tools,
                    $"/{toolID}/{toolName.ToLower().Replace(" ", "")}{imageExtension}",
                    imageBytes);
                await toolService.UpdateToolImageUrl(toolID, imageUrl);
            }
        }
    }
}
