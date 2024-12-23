using HtmlAgilityPack;
using SearchAiDirectory.Shared.AiAgents;
using SearchAiDirectory.Shared.Models;
using SearchAiDirectory.Shared.Utilities;

namespace SearchAiDirectory.Function.Functions;

public class WriteAboutLatestNews(INewsService newsService)
{
    [Function("WriteAboutLatestNews")]
    public async Task Run([TimerTrigger("0 0 11 * * 1")] TimerInfo myTimer)
    {
        var weeklyNews = await SerperService.GetWeeklyNews();
        foreach (var news in weeklyNews)
        {
            float uniqueness = 1;
            string newsTitle = news.title;
            string websiteContent = await GetWebsiteContentAsync(news.link);

            if (string.IsNullOrEmpty(websiteContent) || websiteContent.Length < 300) continue;

            var newsEmbeddingCode = await OpenAiService.GetEmbedding($"{newsTitle}|{websiteContent}");
            var embedding = await newsService.GetNewsEmbeddingValues(newsEmbeddingCode, 1);
            uniqueness = embedding.ToDictionary(r => r.Key, r => r.Value).FirstOrDefault().Value;

            if (uniqueness > 0.71) continue;

            var newNews = new News();
            var aiAgent = new AgentBase("News Writer", .7, SystemPrompts.WebsiteFAQ);
            aiAgent.SetupWebsiteContentForAi("This is a news article for which you will anwser question:\n" + websiteContent);

            newNews.Title = await aiAgent.DirectOpenAiResponse("Create a catchy title for this new article. Return Only the finalized title, without any additional context.");
            newNews.Title = newNews.Title.Replace("\"", "").Replace("\'", "").Trim().Normalize();
            newNews.Slug = RegexHelper.TextToSlug(newNews.Title);
            newNews.Content = await aiAgent.DirectOpenAiResponse("Summarize this news article into simplified and catchy tune like your talking to someone. Return Only the finalized summary, without any additional context.");
            newNews.Content = newNews.Content.Replace("\"", "").Replace("\'", "").Trim().Normalize();
            newNews.MetaDescription = await aiAgent.DirectOpenAiResponse("Give me a meta description less than 140 character for this tool. no pre context or no post context is needed.");
            newNews.MetaDescription = newNews.MetaDescription.Replace("\"", "").Replace("\'", "").Trim().Normalize();
            newNews.MetaKeywords = await aiAgent.DirectOpenAiResponse("Give me a meta keywords less than 140 character in comma delimited format for this tool. no pre context or no post context is needed.");
            newNews.MetaKeywords = newNews.MetaKeywords.Replace("\"", "").Replace("\'", "").Trim().Normalize();
            newNews.Website = news.link;
            newNews.WebsiteContent = websiteContent;
            newNews = await newsService.AddNews(newNews);

            var aiImagePrompt = await aiAgent.DirectOpenAiResponse($"Create a photorealistic image prompt that is relateable to the reader of the following News Summary." +
                $"\nThe news title is '{newNews.Title}'." +
                $"\nFollowing is the summary of the news: {newNews.Content}" +
                $"\nAvoid infographics. Use a modern and minimalist style. Include detailed descriptions of the Subject, Colors, Mood, Lighting, Composition, and Camera settings to achieve super-realistic imagery." +
                $"\nNo pre-context or post-context is needed; just provide the finalized prompt to generate the image.");

            newNews.ImageUrl = await DownloadImageAndUploadToAzure(newNews.ID, aiImagePrompt);
            await newsService.UpdateNews(newNews);
            await newsService.CreateNewsEmbedding(newNews.ID, newsEmbeddingCode);
        }
    }

    private static async Task<string> GetWebsiteContentAsync(string link)
    {
        using var client = new HttpClient();
        var response = client.GetAsync(link).Result;
        if (response.IsSuccessStatusCode)
        {
            string htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Assuming the main content is within <article> tags
            var articleNode = htmlDocument.DocumentNode.SelectSingleNode("//article");
            if (articleNode is not null)
            {
                return RegexHelper.HtmlToCleanText(articleNode.InnerText);
            }
            else
            {
                // Fallback to body content if article tag is not found
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                return bodyNode != null ? RegexHelper.HtmlToCleanText(bodyNode.InnerText) : string.Empty;
            }
        }
        else
        {
            return string.Empty;
        }
    }

    public static async Task<string> DownloadImageAndUploadToAzure(long newsID, string imagePrompt)
    {
        if (!string.IsNullOrEmpty(imagePrompt))
        {
            var imageUrl = await OpenAiService.GetAiImage(imagePrompt);
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes is null) return null;

            var imageExtension = Path.GetExtension(imageUrl);
            var azImageUrl = await AzureStorageService.UploadBlobFiles(
                BlobContainers.news,
                $"/{newsID}{imageExtension}",
                imageBytes);

            return azImageUrl;
        }

        return string.Empty;
    }
}
