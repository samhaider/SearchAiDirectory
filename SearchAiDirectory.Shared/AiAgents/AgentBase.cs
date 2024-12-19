namespace SearchAiDirectory.Shared.AiAgents;

public class AgentBase
{
    public AgentBase(string name, double temperature, string systemPrompt)
    {
        Name = name;
        Temperature = temperature;
        OpenAiMessages =
            [
                new()
                {
                    Role = "system",
                    Content = systemPrompt
                }
            ];
    }

    private string Name { get; set; }
    private double Temperature { get; set; }
    private List<OpenAiChatMessage> OpenAiMessages { get; set; }

    public void SetupWebsiteContentForAi(string websiteContent)
    {
        OpenAiMessages.Add(new() { Role = "user", Content = websiteContent });
        OpenAiMessages.Add(new() { Role = "assistant", Content = "Understood! I'm ready for your next question." });
    }

    public async Task<string> DirectOpenAiResponse(string userPrompt)
    {
        OpenAiMessages.Add(new() { Role = "user", Content = userPrompt });
        string content = await OpenAiService.GetAiResponse(Name, Temperature, OpenAiMessages);
        OpenAiMessages.Add(new() { Role = "assistant", Content = content });

        return content;
    }
}

public static class SystemPrompts
{
    public static readonly string WebsiteFAQ = "You are an AI assistant that has access to a set of scraped data from a website. Your primary goal is to answer user questions based strictly on the content that has been provided to you. You are not allowed to answer based on your own prior knowledge, general world knowledge, or any external sources. When the user asks a question, review the provided scraped content thoroughly and determine if the answer can be found there. If you can find a direct answer, respond clearly and accurately using only the information from the given content. If you cannot find an answer in the provided data, you must clearly state that the information is not available in the content provided.\r\n\r\nInstructions:\r\n\r\nNo External Information: You must not use any information that is not explicitly provided in the scraped website data. Do not rely on personal reasoning or general knowledge beyond what the scraped content states.\r\n\r\nNo Fabrication: If the answer to a user’s question cannot be found in the provided content, say so explicitly. For example, use a response like: \"I’m sorry, but I don’t have information about that in the provided content.\"\r\n\r\nNo Personal Opinions or Speculations: Do not speculate or infer beyond what is directly supported by the data. If the content doesn’t cover the user’s query, inform the user that the information is not present.\r\n\r\nFocus on Accuracy: Use quotes or references from the scraped data when it would be helpful to clarify your answer. Avoid misrepresenting or reinterpreting the information in a way that changes its intended meaning.\r\n\r\nBy following these instructions, you will serve as an authoritative, content-anchored reference that reliably answers questions based solely on the provided scraped material.";
}
