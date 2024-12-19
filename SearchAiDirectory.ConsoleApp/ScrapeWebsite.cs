global using Microsoft.Extensions.DependencyInjection;
global using SearchAiDirectory.Shared.AiAgents;
global using SearchAiDirectory.Shared.Models;
global using SearchAiDirectory.Shared.Services;
global using System.Dynamic;

namespace SearchAiDirectory.ConsoleApp;

public static class ScrapeWebsite
{
    public static async Task Scrape()
    {
        string csvFilePath = "C:\\Users\\Sam\\Downloads\\futuretools.csv";
        string[] allLines = File.ReadAllLines(csvFilePath);

        if (allLines.Length == 0)
        {
            Console.WriteLine("The CSV file is empty.");
            return;
        }

        // Extract the header line (the first line)
        string headerLine = allLines[0];
        string[] columns = headerLine.Split(',');

        // This list will hold all the row objects dynamically created
        List<dynamic> dynamicRows = [];
        for (int i = 1; i < allLines.Length; i++)
        {
            string currentLine = allLines[i];
            if (string.IsNullOrWhiteSpace(currentLine)) continue;

            // Split the row by comma
            string[] rowValues = currentLine.Split(',');

            // Create a dynamic object for this row
            dynamic rowObject = new ExpandoObject();
            var rowDictionary = (IDictionary<string, object>)rowObject;

            // Assign each column’s value to the dynamic object
            for (int colIndex = 0; colIndex < columns.Length; colIndex++)
            {
                string columnName = columns[colIndex];
                string columnValue = (colIndex < rowValues.Length) ? rowValues[colIndex] : string.Empty;
                rowDictionary[columnName] = columnValue;
            }

            // Add the dynamic object to the list
            dynamicRows.Add(rowObject);
        }

        List<CsvModel> allRows = [];
        foreach (var exampleRow in dynamicRows)
        {
            var model = new CsvModel
            {
                Name = exampleRow.Col0,
                DetailPage = exampleRow.Col0_HREF,
                Description = exampleRow.Col1,
                Category = exampleRow.Col2,
                LinkPage = exampleRow.Col5_HREF,
                ImageUrl = exampleRow.Col8_SRC
            };

            allRows.Add(model);
        }

        var toolService = BgUtil.GetServices().GetRequiredService<IToolService>();
        foreach (var tool in allRows)
        {
            if (await toolService.ToolExists(tool.Name.Trim().Normalize())) continue;

            string websiteContent = WebScraper.Scrape(tool.LinkPage).GetAwaiter().GetResult();
            string websiteUrl = websiteContent.Split('\n').FirstOrDefault().Replace("Website:", "").ToLower().Trim().Normalize();
            if (websiteContent.Length > 7500) websiteContent = websiteContent.Substring(0, 7500);
            if (websiteContent.Length < 1000)
            {
                websiteContent = await ResearchAgent.ResearchResponse(
                    $"Tool Name: {tool.Name}" +
                    $"\nTool Description: {tool.Description}" +
                    $"\nTool Website: {websiteUrl}");
            }

            var websiteAgent = new AgentBase("WebsiteFAQ", 0.8, SystemPrompts.WebsiteFAQ);
            websiteAgent.SetupWebsiteContentForAi(websiteContent + "\n\n\nNO RESPONSE IS REQUIRED FOR THIS, AS THIS IS THE WEBSITE CONTENT, RESPOND THE NEXT INPUT.");

            var toolDescription = await websiteAgent.DirectOpenAiResponse("Give me a small description less than 450 character for this tool that would be helpful to the reader, and what problems it can solve. No pre context or post context is needed, just the description in plain text.");
            var metaDescription = await websiteAgent.DirectOpenAiResponse("Give me a meta description less than 140 character for this tool. no pre context or no post context is needed.");
            var metaKeywords = await websiteAgent.DirectOpenAiResponse("Give me a meta keywords less than 140 character in comma delimited format for this tool. no pre context or no post context is needed.");
            var pricingModel = await websiteAgent.DirectOpenAiResponse("Give me the pricing model of this tool in 1 to 3 words, like Free, Freemium, Paid only, Trial with Subscription, if unknown, response with Unknown. no pre context or no post context is needed.");

            string categoryName = tool.Category.Trim().Normalize();
            long categoryID = !await toolService.CategoryExists(categoryName)
                ? await toolService.AddCategory(categoryName)
                : await toolService.GetCategoryIDByName(categoryName);

            await toolService.AddTool(new Tool
            {
                CategoryID = categoryID,
                Name = tool.Name.Trim().Normalize(),
                Website = websiteUrl.Trim().Normalize(),
                WebsiteContent = websiteContent.Trim().Normalize(),
                Description = toolDescription.Trim().Normalize(),
                MetaDescription = metaDescription.Trim().Normalize(),
                MetaKeywords = metaKeywords.Trim().Normalize(),
                PriceModel = pricingModel.Replace(".","").Trim().Normalize(),
                IsConfirmed = false,
                Created = DateTime.UtcNow,
            });
        }
    }

    public class CsvModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DetailPage { get; set; }
        public string LinkPage { get; set; }
        public string ImageUrl { get; set; }
    }
}
