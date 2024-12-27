namespace SearchAiDirectory.Shared.ThirdPartyService;

public static class PerplexityService
{
    private static readonly string PerlexityApiKey = "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";
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static async Task<string> Research(List<PerplexityMessage> messages)
    {
        var perplexityRequest = new PerplexityRequest() { messages = messages };
        var content = JsonSerializer.Serialize(perplexityRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.perplexity.ai/chat/completions";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {PerlexityApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<PerplexityResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult.choices?.FirstOrDefault()?.message?.content.Trim().Normalize();
    }
}

#pragma warning disable IDE1006 // Naming Styles

public class PerplexityRequest
{
    public string model { get; } = "llama-3.1-sonar-large-128k-online";
    public List<PerplexityMessage> messages { get; set; }
    public string max_tokens { get; set; }
    public bool return_images { get; } = false;
    public bool return_related_questions { get; } = false;
}

public class PerplexityMessage
{
    public string role { get; set; }
    public string content { get; set; }
}


public class PerplexityResponse
{
    public string id { get; set; }
    public string model { get; set; }
    public string _object { get; set; }
    public int created { get; set; }
    public string[] citations { get; set; }
    public PerplexityChoice[] choices { get; set; }
    public PerplexityUsage usage { get; set; }
}

public class PerplexityUsage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

public class PerplexityChoice
{
    public int index { get; set; }
    public string finish_reason { get; set; }
    public PerplexityMessage message { get; set; }
    public PerplexityDelta delta { get; set; }
}

public class PerplexityDelta
{
    public string role { get; set; }
    public string content { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles