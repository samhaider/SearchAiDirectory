namespace SearchAiDirectory.Shared.Services;

public interface IToolService
{
    Task<IList<Tool>> GetAllTools();
    Task<bool> ToolExists(string name);
    Task<Tool> GetToolByID(long toolID);
    Task<Tool> GetToolByName(string toolName);
    Task<Tool> GetToolBySlug(string toolSlug);
    Task<Tool> AddTool(Tool newTool);
    Task UpdateTool(Tool updatedTool);
    Task DeleteTool(long toolID);
    Task ChangeCategory(long toolID, long categoryID);
    Task UpdateToolImageUrl(long toolID, string imageUrl);
    Task<IList<Tool>> GetTop3Tools();
    Task CreateEmbeddingRecord(long toolID);
    Task DeleteEmbeddingRecord(long toolID);
    Task<KeyValuePair<long, float>[]> ToolsSearchEmbeddings(float[] queryEmbeddingCode, int topN);
    Task<IList<Tool>> EmbeddingSearchTools(float[] queryEmbeddingCode, int topN);
    Task<IList<Tool>> Get3RelatedTools(long toolID, string toolEmbeddingCode);
}

public class ToolService(IDbContextFactory<ApplicationDataContext> dbContextFactory) : IToolService
{
    public async Task<IList<Tool>> GetAllTools()
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tools.OrderByDescending(o => o.Created).ToListAsync();
    }

    public async Task<bool> ToolExists(string name)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        name = name.ToLower().Trim().Normalize();
        return await db.Tools.AnyAsync(a => a.Name.ToLower() == name || a.Description.ToLower().Contains(name) || a.MetaDescription.ToLower().Contains(name) || a.MetaKeywords.ToLower().Contains(name));
    }

    public async Task<Tool> GetToolByID(long toolID)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tools
            .Where(w => w.ID == toolID)
            .Include(i => i.Category)
            .Include(i => i.Likes)
            .Include(i => i.Comments)
            .SingleOrDefaultAsync();
    }

    public async Task<Tool> GetToolByName(string toolName)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tools.Include(i => i.Category).Where(w => w.Name.ToLower() == toolName.ToLower()).SingleOrDefaultAsync();
    }

    public async Task<Tool> GetToolBySlug(string toolSlug)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tools
            .Where(db => db.Slug == toolSlug)
            .Include(i => i.Category)
            .Include(i => i.Likes)
            .Include(i => i.Comments).ThenInclude(i => i.User)
            .Include(i => i.Embedding)
            .SingleOrDefaultAsync();
    }

    public async Task<Tool> AddTool(Tool newTool)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        
        newTool.Slug = RegexHelper.TextToSlug(newTool.Name);
        newTool.LikeCount = 0;
        newTool.Created = DateTime.UtcNow;
        await db.Tools.AddAsync(newTool);
        await db.SaveChangesAsync();
        return newTool;
    }

    public async Task UpdateTool(Tool updateTool)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        updateTool.Modified = DateTime.UtcNow;
        db.Tools.Update(updateTool);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTool(long toolID)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Tools.Include(i => i.Embedding).Where(w => w.ID == toolID).ExecuteDeleteAsync();
    }

    public async Task UpdateToolImageUrl(long toolID, string imageUrl)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageUrl, imageUrl));
    }

    public async Task<IList<Tool>> GetTop3Tools()
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tools.OrderByDescending(o => o.Created).Take(3).ToListAsync();
    }

    public async Task ChangeCategory(long toolID, long categoryID)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Tools.Where(w => w.ID == toolID).ExecuteUpdateAsync(u => u.SetProperty(s => s.CategoryID, categoryID));
    }

    public async Task<KeyValuePair<long, float>[]> ToolsSearchEmbeddings(float[] queryEmbeddingCode, int topN)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        var embedding = await db.Embeddings.ToListAsync();

        return embedding.Select(embeddingCode => new KeyValuePair<long, float>(
            key: embeddingCode.ToolID,
            value: TensorPrimitives.CosineSimilarity(
                new ReadOnlySpan<float>(JsonSerializer.Deserialize<float[]>(embeddingCode.EmbeddingCode)),
                new ReadOnlySpan<float>(queryEmbeddingCode))))
            .OrderByDescending(match => match.Value)
            .Take(topN)
            .ToArray();
    }

    public async Task CreateEmbeddingRecord(long toolID)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        if (await db.Embeddings.AnyAsync(a => a.ToolID == toolID)) return;

        var tool = await db.Tools.Include(i => i.Category).SingleOrDefaultAsync(s => s.ID == toolID);
        var combinedText = $"{tool.Name}|{tool.Description}|{tool.WebsiteContent}";

        var newEmbedding = await OpenAiService.GetEmbedding(combinedText);
        if (newEmbedding is null) return;

        var embedding = new Embedding
        {
            ToolID = tool.ID,
            EmbeddingCode = JsonSerializer.Serialize(newEmbedding),
            CreatedOn = DateTime.UtcNow,
        };

        await db.Embeddings.AddAsync(embedding);
        await db.SaveChangesAsync();
    }

    public async Task DeleteEmbeddingRecord(long toolID)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Embeddings.Where(w => w.ToolID == toolID).ExecuteDeleteAsync();
    }

    public async Task<IList<Tool>> EmbeddingSearchTools(float[] queryEmbeddingCode, int topN)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        var result = await ToolsSearchEmbeddings(queryEmbeddingCode, topN);
        var relatedTools = result.Select(s => s.Key).ToList();
        var tools = await db.Tools.Where(w => relatedTools.Contains(w.ID)).ToListAsync();

        var orderDictionary = result.ToDictionary(r => r.Key, r => r.Value);
        return [.. tools.OrderByDescending(o => orderDictionary[o.ID])];
    }

    public async Task<IList<Tool>> Get3RelatedTools(long toolID, string toolEmbeddingCode)
    {
        using var db = await dbContextFactory.CreateDbContextAsync();
        var queryEmbeddingCode = JsonSerializer.Deserialize<float[]>(toolEmbeddingCode);

        var result = await ToolsSearchEmbeddings(queryEmbeddingCode, 4);
        var relatedTools = result.Select(s => s.Key).ToList();
        var tools = await db.Tools.Include(i => i.Category).Where(w => w.ID != toolID).Where(w => relatedTools.Contains(w.ID)).ToListAsync();

        var orderDictionary = result.ToDictionary(r => r.Key, r => r.Value);
        return [.. tools.OrderByDescending(o => orderDictionary[o.ID])];
    }
}