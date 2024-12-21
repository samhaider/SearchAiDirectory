using System.Numerics.Tensors;

namespace SearchAiDirectory.Shared.Services;

public interface IEmbeddingService
{
    Task CreateEmbeddingRecord(long toolID);
    Task<KeyValuePair<long, float>[]> SearchEmbeddings(float[] queryEmbeddingCode, int topN);
    Task<IList<Tool>> EmbeddingSearchTools(float[] queryEmbeddingCode, int topN);
}

public class EmbeddingService(ApplicationDataContext db) : IEmbeddingService
{
    public async Task CreateEmbeddingRecord(long toolID)
    {
        if (await db.Embeddings.AnyAsync(a => a.ToolID == toolID)) return;

        var tool = await db.Tools.Include(i => i.Category).SingleOrDefaultAsync(s => s.ID == toolID);
        var combinedText = $"{tool.Name}|{tool.Description}|{tool.WebsiteContent}";

        var newEmbedding = await OpenAiService.GetEmbedding(combinedText);
        if (newEmbedding is null) return;

        var embedding = new ToolEmbedding
        {
            ToolID = tool.ID,
            EmbeddingCode = JsonSerializer.Serialize(newEmbedding),
            CreatedOn = DateTime.UtcNow,
        };

        await db.Embeddings.AddAsync(embedding);
        await db.SaveChangesAsync();
    }

    public async Task<KeyValuePair<long, float>[]> SearchEmbeddings(float[] queryEmbeddingCode, int topN)
    {
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


    public async Task<IList<Tool>> EmbeddingSearchTools(float[] queryEmbeddingCode, int topN)
    {
        var result = await SearchEmbeddings(queryEmbeddingCode, topN);
        var relatedTools = result.Select(s => s.Key).ToList();
        var tools = await db.Tools.Where(w => relatedTools.Contains(w.ID)).ToListAsync();

        var orderDictionary = result.ToDictionary(r => r.Key, r => r.Value);
        return [.. tools.OrderByDescending(o => orderDictionary[o.ID])];
    }
}