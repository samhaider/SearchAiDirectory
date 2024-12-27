namespace SearchAiDirectory.Shared.ThirdPartyService;

public static class OpenAiService
{
    private static readonly string openaiApiKey = "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";
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static async Task<string> GetAiResponse(string userName, double temperature, List<OpenAiChatMessage> messages)
    {
        var aiRequest = new OpenAiChatRequest()
        {
            Messages = messages,
            Temperature = temperature,
            User = userName
        };
        var content = JsonSerializer.Serialize(aiRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.openai.com/v1/chat/completions";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<OpenAiChatResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult.choices?.FirstOrDefault()?.message?.Content.Trim().Normalize();
    }

    public static async Task<string> GetAiImage(string prompt)
    {
        var aiImageRequest = new OpenAiImageRequest() { prompt = prompt };
        var content = JsonSerializer.Serialize(aiImageRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.openai.com/v1/images/generations";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<OpenAiImageResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult.data?.FirstOrDefault()?.url.Trim().Normalize();
    }

    public static async Task<byte[]> GetAiSpeech(string input, OpenAiModels openAiModel)
    {
        var aiSpeechRequest = new OpenAiSpeechRequest
        {
            input = input,
            model = openAiModel.Value,
            voice = OpenAiSpeechVoice.Echo.Value
        };
        var content = JsonSerializer.Serialize(aiSpeechRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.openai.com/v1/audio/speech";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    public static async Task<string> GetAiVisionResponse(string request, string[] imageUrls)
    {
        var visionContent = new List<OpenAiVisionContent>() { new() { type = "text", text = request } };
        var n = 1;
        foreach (var imageUrl in imageUrls)
        {
            visionContent.Add(new() { type = "text", text = n.ToString() });
            visionContent.Add(new OpenAiVisionContent
            {
                type = "image_url",
                image_url = new OpenAiVisionImageUrl
                {
                    url = imageUrl,
                    detail = "high"
                }
            });
            n++;
        };
        var aiVisionRequest = new OpenAiVisionRequest
        {
            model = OpenAiModels.Gpt4oMini.Value,
            messages = [
                new OpenAiVisionMessage { role = "user", content = visionContent }
            ],
            max_tokens = 300
        };
        var content = JsonSerializer.Serialize(aiVisionRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.openai.com/v1/chat/completions";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<OpenAiChatResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult.choices?.FirstOrDefault()?.message?.Content.Trim().Normalize();
    }

    public static async Task<float[]> GetEmbedding(string input)
    {
        var embeddingRequest = new OpenAiEmbeddingRequest() { input = TruncateTextToEmbeddingTokenLimit(input) };
        var content = JsonSerializer.Serialize(embeddingRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        string apiUrl = "https://api.openai.com/v1/embeddings";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.PostAsync(apiUrl, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<OpenAiEmbeddingResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult.data?.FirstOrDefault()?.embedding.ToArray();
    }
    private static string TruncateTextToEmbeddingTokenLimit(string text)
    {
        // Maximum number of tokens for GPT-4o is 8191
        int maxTokens = 8000;

        // Initialize the SharpToken tokenizer for the model, e.g., "gpt-3.5-turbo"
        var tokenizer = SharpToken.GptEncoding.GetEncodingForModel(OpenAiModels.Embedding.Value);

        // Tokenize the text
        var tokens = tokenizer.Encode(text);

        // Check if the number of tokens exceeds the limit
        if (tokens.Count > maxTokens)
        {
            // Truncate the tokens to the desired length
            var truncatedTokens = tokens.Take(maxTokens).ToList();

            // Decode the tokens back to a string
            return tokenizer.Decode(truncatedTokens);
        }
        else
        {
            return text;
        }
    }

    public static async Task<List<(string Word, TimeSpan StartTime, TimeSpan EndTime)>> GetSpeechToTextForSubTitles(StreamContent audioFile)
    {
        string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {openaiApiKey}");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new MultipartFormDataContent
        {
            { audioFile },
            { new StringContent("whisper-1"), "model" },
            { new StringContent("word"), "timestamp_granularities[]" },
            { new StringContent("verbose_json"), "response_format" }
        };
        using var response = await httpClient.PostAsync(apiUrl, content);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<OpenAiTranscriptionsResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult?.words?.Select(s => (s.word, TimeSpan.FromSeconds(s.start), TimeSpan.FromSeconds(s.end))).ToList();
    }
}

#pragma warning disable IDE1006 // Naming Styles
public class OpenAiModels
{
    private OpenAiModels(string value) { Value = value; }
    public string Value { get; private set; }

    public static OpenAiModels Gpto1 { get { return new OpenAiModels("gpt-4o"); } }
    public static OpenAiModels Gpt4oMini { get { return new OpenAiModels("gpt-4o-mini"); } }
    public static OpenAiModels Embedding { get { return new OpenAiModels("text-embedding-3-large"); } }
    public static OpenAiModels Text2Speech { get { return new OpenAiModels("tts-1"); } }
    public static OpenAiModels Text2SpeechHD { get { return new OpenAiModels("tts-1-hd"); } }
}

public class OpenAiChatRequest
{
    public string Model { get; } = OpenAiModels.Gpt4oMini.Value;
    public List<OpenAiChatMessage> Messages { get; set; }
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public int? N { get; set; }
    public bool? Stream { get; set; }
    public List<string> Stop { get; set; }
    public int? MaxTokens { get; set; }
    public double? PresencePenalty { get; set; }
    public double? FrequencyPenalty { get; set; }
    public Dictionary<string, double> LogitBias { get; set; }
    public string User { get; set; }
}
public class OpenAiChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
}
public class OpenAiChatUsage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}
public class OpenAiChatChoice
{
    public OpenAiChatMessage message { get; set; }
    public string finish_reason { get; set; }
    public int index { get; set; }
}
public class OpenAiChatResponse
{
    public string id { get; set; }
    public string _object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public OpenAiChatUsage usage { get; set; }
    public OpenAiChatChoice[] choices { get; set; }
}

public class OpenAiImageRequest
{
    public string model { get; } = "dall-e-3";
    public string size { get; } = "1024x1024";
    public string quality { get; } = "hd";
    public string prompt { get; set; }
    public int n { get; } = 1;

}
public class OpenAiImageResponse
{
    public int created { get; set; }
    public OpenAiImageData[] data { get; set; }
}
public class OpenAiImageData
{
    public string url { get; set; }
}

public class OpenAiSpeechRequest
{
    public string model { get; set; }
    public string input { get; set; }
    public string voice { get; set; }
}
public class OpenAiSpeechVoice
{
    private OpenAiSpeechVoice(string value) { Value = value; }
    public string Value { get; private set; }

    public static OpenAiSpeechVoice Alloy { get { return new OpenAiSpeechVoice("alloy"); } }
    public static OpenAiSpeechVoice Echo { get { return new OpenAiSpeechVoice("echo"); } }
    public static OpenAiSpeechVoice Fable { get { return new OpenAiSpeechVoice("fable"); } }
    public static OpenAiSpeechVoice Onyx { get { return new OpenAiSpeechVoice("onyx"); } }
    public static OpenAiSpeechVoice Nova { get { return new OpenAiSpeechVoice("nova"); } }
    public static OpenAiSpeechVoice Shimmer { get { return new OpenAiSpeechVoice("shimmer"); } }
}

public class OpenAiVisionRequest
{
    public string model { get; set; }
    public List<OpenAiVisionMessage> messages { get; set; }
    public int max_tokens { get; set; }
}
public class OpenAiVisionMessage
{
    public string role { get; set; }
    public List<OpenAiVisionContent> content { get; set; }
}
public class OpenAiVisionContent
{
    public string type { get; set; }
    public string text { get; set; }
    public OpenAiVisionImageUrl image_url { get; set; }
}
public class OpenAiVisionImageUrl
{
    public string url { get; set; }
    public string detail { get; set; }
}

public class OpenAiEmbeddingRequest
{
    public string input { get; set; }
    public string model { get; } = "text-embedding-3-large";
    public string encoding_format { get; } = "float";
}
public class OpenAiEmbeddingResponse
{
    public string _object { get; set; }
    public List<OpenAiEmbeddingData> data { get; set; }
    public string model { get; set; }
    public OpenAiEmbeddingUsage usage { get; set; }
}
public class OpenAiEmbeddingData
{
    public string _object { get; set; }
    public List<float> embedding { get; set; }
    public int index { get; set; }
}
public class OpenAiEmbeddingUsage
{
    public int prompt_tokens { get; set; }
    public int total_tokens { get; set; }
}



public class OpenAiTranscriptionsResponse
{
    public string task { get; set; }
    public string language { get; set; }
    public float duration { get; set; }
    public string text { get; set; }
    public OpenAiTranscriptionsResponseWord[] words { get; set; }
}

public class OpenAiTranscriptionsResponseWord
{
    public string word { get; set; }
    public double start { get; set; }
    public double end { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles