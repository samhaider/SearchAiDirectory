namespace SearchAiDirectory.Shared.Services;

public interface INewsService
{
    Task<News> AddNews(News newNews);
    Task<List<News>> GetAllNews();
    Task<News> GetNewsByID(long ID);
    Task<News> GetNewsBySlug(string slug);
    Task<bool> NewsEmbeddingExsits(long newsID);
    Task UpdateNews(News updatedNews);

    Task CreateNewsEmbedding(long newsID);
    Task<KeyValuePair<long, float>[]> GetNewsEmbeddingValues(float[] queryEmbeddingCode, int topN);
    Task<IList<News>> NewsEmbeddingSearch(float[] queryEmbeddingCode, int topN);
    Task<IList<News>> Get3RelatedNews(long newsID, string newsEmbeddingCode);
}

public class NewsService(IDbContextFactory<ApplicationDataContext> dbContextFactory) : INewsService
{
    public async Task<News> AddNews(News newNews)
    {
        using var context = dbContextFactory.CreateDbContext();

        newNews.Created = DateTime.UtcNow;
        await context.News.AddAsync(newNews);
        await context.SaveChangesAsync();
        return newNews;
    }

    public async Task UpdateNews(News updatedNews)
    {
        using var context = dbContextFactory.CreateDbContext();

        updatedNews.Modified = DateTime.UtcNow;
        context.News.Update(updatedNews);
        await context.SaveChangesAsync();
    }

    public async Task<News> GetNewsBySlug(string slug)
    {
        using var context = dbContextFactory.CreateDbContext();

        return await context.News.Include(i => i.Embedding).FirstOrDefaultAsync(n => n.Slug == slug);
    }

    public async Task<News> GetNewsByID(long ID)
    {
        using var context = dbContextFactory.CreateDbContext();
        return await context.News.Include(i => i.Embedding).SingleOrDefaultAsync(n => n.ID == ID);
    }

    public async Task<List<News>> GetAllNews()
    {
        using var context = dbContextFactory.CreateDbContext();
        return await context.News.OrderByDescending(o => o.Created).ToListAsync();
    }


    public async Task<bool> NewsEmbeddingExsits(long newsID)
    {
        using var context = dbContextFactory.CreateDbContext();
        return await context.NewsEmbeddings.AnyAsync(n => n.NewsID == newsID);
    }

    public async Task CreateNewsEmbedding(long newsID)
    {
        using var context = dbContextFactory.CreateDbContext();
        if (await context.NewsEmbeddings.AnyAsync(a => a.NewsID == newsID)) return;

        var news = await context.News.SingleOrDefaultAsync(s => s.ID == newsID);
        var combinedText = $"{news.Title}|{news.Content}|{news.WebsiteContent}";

        var newEmbeddingCode = await OpenAiService.GetEmbedding(combinedText);
        if (newEmbeddingCode is null) return;

        var newsEmbedding = new NewsEmbedding()
        {
            NewsID = newsID,
            EmbeddingCode = JsonSerializer.Serialize(newEmbeddingCode),
            CreatedOn = DateTime.UtcNow
        };

        await context.NewsEmbeddings.AddAsync(newsEmbedding);
        await context.SaveChangesAsync();
    }

    public async Task<KeyValuePair<long, float>[]> GetNewsEmbeddingValues(float[] queryEmbeddingCode, int topN)
    {
        using var db = dbContextFactory.CreateDbContext();
        var embedding = await db.NewsEmbeddings.ToListAsync();

        return embedding.Select(embeddingCode => new KeyValuePair<long, float>(
            key: embeddingCode.NewsID,
            value: TensorPrimitives.CosineSimilarity(
                new ReadOnlySpan<float>(JsonSerializer.Deserialize<float[]>(embeddingCode.EmbeddingCode)),
                new ReadOnlySpan<float>(queryEmbeddingCode))))
            .OrderByDescending(match => match.Value)
            .Take(topN)
            .ToArray();
    }

    public async Task<IList<News>> NewsEmbeddingSearch(float[] queryEmbeddingCode, int topN)
    {
        using var db = dbContextFactory.CreateDbContext();
        var result = await GetNewsEmbeddingValues(queryEmbeddingCode, topN);
        var relatedNews = result.Select(s => s.Key).ToList();
        var news = await db.News.Where(w => relatedNews.Contains(w.ID)).ToListAsync();

        var orderDictionary = result.ToDictionary(r => r.Key, r => r.Value);
        return [.. news.OrderByDescending(o => orderDictionary[o.ID])];
    }

    public async Task<IList<News>> Get3RelatedNews(long newsID, string newsEmbeddingCode)
    {
        using var db = dbContextFactory.CreateDbContext();
        var queryEmbeddingCode = JsonSerializer.Deserialize<float[]>(newsEmbeddingCode);

        var result = await GetNewsEmbeddingValues(queryEmbeddingCode, 4);
        var relatedNews = result.Select(s => s.Key).ToList();
        var news = await db.News.Where(w => w.ID != newsID).Where(w => relatedNews.Contains(w.ID)).ToListAsync();

        var orderDictionary = result.ToDictionary(r => r.Key, r => r.Value);
        return [.. news.OrderByDescending(o => orderDictionary[o.ID])];
    }
}
