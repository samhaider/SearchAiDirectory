namespace SearchAiDirectory.Function.Utils;

public static class ImageUtils
{
    public static async Task<string> CreateImageAndUploadToAzure(string openaiImagePrompt, long newsID)
    {
        if (string.IsNullOrEmpty(openaiImagePrompt)) return null;

        var imageUrl = await OpenAiService.GetAiImage(openaiImagePrompt);
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
}