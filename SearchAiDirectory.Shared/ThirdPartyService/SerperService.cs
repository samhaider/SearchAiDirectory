namespace SearchAiDirectory.Shared.ThirdPartyService;

public class SerperService
{
    private static readonly string SerperApiKey = "a122b498501525df77e9dbfc3960b3f291c3fff2";
    private static readonly string SerperNewsEndpoint = "https://google.serper.dev/news";
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static async Task<List<SerperNews>> GetWeeklyNews()
    {
        var serperRequest = new SerperRequest();
        var content = JsonSerializer.Serialize(serperRequest, jsonOptions);
        var data = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", SerperApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.PostAsync(SerperNewsEndpoint, data);
        if (!response.IsSuccessStatusCode) return null;

        var responseResult = await JsonSerializer.DeserializeAsync<SerperResponse>(await response.Content.ReadAsStreamAsync(), jsonOptions);
        return responseResult?.news?.ToList();
    }

#pragma warning disable IDE1006 // Naming Styles
    internal class SerperRequest
    {
        public string q { get; } = "Ai, Ai Tools, Ai Saas";
        public string location { get; } = "United States";
        public string tbs { get; } = "qdr:w";
    }
    internal class SerperResponse
    {
        public SerperSearchParameters searchParameters { get; set; }
        public SerperNews[] news { get; set; }
        public int credits { get; set; }
    }
    internal class SerperSearchParameters
    {
        public string q { get; set; }
        public string type { get; set; }
        public string tbs { get; set; }
        public string location { get; set; }
        public string engine { get; set; }
        public string gl { get; set; }
    }
    public class SerperNews
    {
        public string title { get; set; }
        public string link { get; set; }
        public string snippet { get; set; }
        public string date { get; set; }
        public string source { get; set; }
        public string imageUrl { get; set; }
        public int position { get; set; }
    }

#pragma warning restore IDE1006 // Naming Styles
}
