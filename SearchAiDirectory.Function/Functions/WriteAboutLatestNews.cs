namespace SearchAiDirectory.Function.Functions;

public class WriteAboutLatestNews(INewsService newsService)
{
    [Function("WriteAboutLatestNews")]
    public async Task Run([TimerTrigger("0 0 11 * * 1")] TimerInfo myTimer)
    {
        var weeklyNews = await SerperService.GetNews(SerperService.WeeklySerperRequest);
        foreach (var news in weeklyNews)
        {
            float uniqueness = 1;
            string newsTitle = news.title;
            string websiteContent = await ScrapeWebsite.GetWebsiteTextContent(news.link);

            if (string.IsNullOrEmpty(websiteContent) || websiteContent.Length < 300) continue;

            var newsEmbeddingCode = await OpenAiService.GetEmbedding($"{newsTitle}|{websiteContent}");
            var embedding = await newsService.GetNewsEmbeddingValues(newsEmbeddingCode, 1);
            uniqueness = embedding.ToDictionary(r => r.Key, r => r.Value).FirstOrDefault().Value;

            if (uniqueness > 0.65) continue;

            var newNews = new News();
            var aiAgent = new AgentBase("News Writer", .7, SystemPrompts.WebsiteFAQ);
            aiAgent.SetupContentAfterInitialization($"\nNews Title: {newsTitle}" + $"\nScraped content from the news website: {websiteContent}");

            newNews.Title = await aiAgent.DirectOpenAiResponse("Create a catchy title for this new article. Return Only the finalized title, without any additional context.");
            newNews.Title = newNews.Title.Replace("\"", "").Replace("\'", "").Trim().Normalize();
            newNews.Slug = RegexHelper.TextToSlug(newNews.Title);
            newNews.Content = await aiAgent.DirectOpenAiResponse("Summarize the full news article into simplified and catchy tune like your talking to someone and expand on how it can help businesses. Return Only the finalized summary, without any additional context.");
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

            newNews.ImageUrl = await ImageUtils.CreateImageAndUploadToAzure(aiImagePrompt, newNews.ID);
            await newsService.UpdateNews(newNews);
            await newsService.CreateNewsEmbedding(newNews.ID);
        }
    }
}