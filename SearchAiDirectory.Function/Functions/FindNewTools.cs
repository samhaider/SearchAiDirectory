using System.Text.RegularExpressions;

namespace SearchAiDirectory.Function.Functions;

public class FindNewTools(IToolService toolService, ICategoryService categoryService)
{
    [Function("FindNewTools")]
    public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
    {
        var weeklyNews = await SerperService.GetNews(SerperService.DailySerperRequest);
        foreach (var news in weeklyNews)
        {
            if (string.IsNullOrEmpty(news.link)) continue;

            string websiteContent = await WebsiteScrapper.GetWebsiteHtmlContent(news.link);
            string newsTitle = news.title;

            if (string.IsNullOrEmpty(websiteContent) || websiteContent.Length < 500) continue;

            var aiAgent = new AgentBase("Tool Finder", .7, SystemPrompts.ToolFinder);
            aiAgent.SetupContentAfterInitialization($"\nNews Title: {newsTitle}" + $"\nScraped content from the news website: {websiteContent}");

            var toolWebsites = await aiAgent.DirectOpenAiResponse("What are all the new AI tools’ websites found in the news article? Give the anwser in comma delimited string no additional context and no additiona quotes or double quotes, just the urls seperated by a comma.");
            if (string.IsNullOrEmpty(toolWebsites) || toolWebsites.Contains("No AI tool", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var website in toolWebsites.Split(","))
                if (RegexHelper.IsValidUrl(website))
                {
                    var toolWebsiteContent = await WebsiteScrapper.ScrapeWebsiteWithInternalLinks(website);

                    var toolAiAgent = new AgentBase("Tool Details", .7, SystemPrompts.ToolDetails);
                    toolAiAgent.SetupContentAfterInitialization($"Scraped content from the ai tool website: {toolWebsiteContent}");

                    var toolName = await toolAiAgent.DirectOpenAiResponse("What is the name of the tool? Give me the name and name only. no pre context or no post context is needed.");
                    if (await toolService.ToolExists(toolName.Trim().Normalize())) continue;

                    if (string.IsNullOrEmpty(toolWebsiteContent) || toolWebsiteContent.Length < 500)
                    {
                        var researchAgent = new ResearchAgent();
                        toolWebsiteContent = await researchAgent.ResearchResponse(
                            $"Tool Name: {toolName}" +
                            $"\nTool Website: {website}");

                        toolAiAgent = new AgentBase("Tool Details", .7, SystemPrompts.ToolDetails);
                        toolAiAgent.SetupContentAfterInitialization($"Scraped content from the ai tool website: {toolWebsiteContent}");
                    }

                    var toolDescription = await toolAiAgent.DirectOpenAiResponse("What is the description of the tool? no pre context or no post context is needed.");
                    var metaDescription = await toolAiAgent.DirectOpenAiResponse("Give me a meta description less than 140 character for this tool. no pre context or no post context is needed.");
                    var metaKeywords = await toolAiAgent.DirectOpenAiResponse("Give me a meta keywords less than 140 character in comma delimited format for this tool. no pre context or no post context is needed.");
                    var pricingModel = await toolAiAgent.DirectOpenAiResponse("Give me the pricing model of this tool in 1 to 3 words, like Free, Freemium, Paid only, Trial with Subscription, if unknown, response with Unknown. no pre context or no post context is needed.");

                    var categoryID = await FindCategory(toolName, toolDescription);
                    var tool = new Tool
                    {
                        CategoryID = categoryID,
                        Name = toolName.Trim().Normalize(),
                        Website = website.Trim().Normalize(),
                        WebsiteContent = toolWebsiteContent.Trim().Normalize(),
                        Description = toolDescription.Trim().Normalize(),
                        MetaDescription = metaDescription.Trim().Normalize(),
                        MetaKeywords = metaKeywords.Trim().Normalize(),
                        PriceModel = pricingModel.Trim().Normalize(),
                    };
                    tool = await toolService.AddTool(tool);

                    try
                    {
                        var websiteImage = await WebsiteScrapper.GetWebsiteScreenshot(website);
                        if (websiteImage is not null)
                        {
                            var imageFilename = $"/{tool.ID}/{tool.Slug}.png";
                            var toolImageUrl = await AzureStorageService.UploadBlobFiles(BlobContainers.tools, imageFilename, websiteImage);
                            tool.ImageUrl = toolImageUrl;
                            await toolService.UpdateTool(tool);
                        }
                    }
                    catch
                    { }
                }
        }
    }

    private async Task<long?> FindCategory(string toolName, string toolDescription)
    {
        var allCategories = await categoryService.GetAllCategories();
        var categoryList = string.Join("\n", allCategories.Select(s => $"ID:{s.ID} |Name:{s.Name}").ToArray());
        var prompt = $"Select or create a category for the ai tool with the following details:" +
            $"\nName: '{toolName}'" +
            $"\nDescription: {toolDescription}" +
            $"\n\nFollowing are the current available categories:" +
            $"{categoryList}" +
            $"\n\nResponse ONLY AND ONLY with the category name, without any additional context.";
        var categoryAgent = new AgentBase("Categorize Ai Tool", .8, SystemPrompts.Categorizing);
        var categoryResponse = await categoryAgent.DirectOpenAiResponse(prompt);
        string categoryName = categoryResponse
            .Replace("\'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace("`", "")
            .Trim().Normalize();

        if (categoryName.Contains("categor", StringComparison.OrdinalIgnoreCase)) return 0;

        long? categoryID = null;
        if (!await categoryService.CategoryExists(categoryName))
        {
            string categorgyMetaDescription = await categoryAgent.DirectOpenAiResponse("Create a meta description for this category without any context. the meta description should be less than 140 characters.");
            string categorgyMetaKeywords = await categoryAgent.DirectOpenAiResponse("Create a comma delimited meta keywords for this category without any context. the meta keywords should be less than 140 characters.");

            categoryID = await categoryService.AddCategory(
                new Category
                {
                    Name = categoryName,
                    MetaDescription = categorgyMetaDescription.Trim().Normalize(),
                    MetaKeywords = categorgyMetaKeywords.Trim().Normalize()
                });
        }
        else
        {
            categoryID = await categoryService.GetCategoryIDByName(categoryName);
        }

        return categoryID;
    }
}
