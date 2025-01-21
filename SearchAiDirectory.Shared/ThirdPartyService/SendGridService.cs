using SendGrid;
using SendGrid.Helpers.Mail;

namespace SearchAiDirectory.Shared.ThirdPartyService;

public static class SendGridService
{
    private static readonly string _apiKey = "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";
    private static readonly string _contactList = "b2463783-1698-4171-ae58-434ca16cf2fc";

    private static async Task<string> GetHtmlContentFromServer(string filePath)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(filePath);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task SendEmailAsync(string toEmail, string toName, string subject, string plainTextContent, string htmlContent)
    {
        var from = new EmailAddress("admin@searchaidirectory.com", "SearchAiDirectory");
        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var client = new SendGridClient(_apiKey);
        await client.SendEmailAsync(msg);
    }

    public static async Task AddContactToList(string email, string name)
    {
        string firstName, lastName;
        var nameParts = name.Split(' ', 2);
        if (nameParts.Length == 2)
        {
            firstName = nameParts[0];
            lastName = nameParts[1];
        }
        else
        {
            firstName = name;
            lastName = string.Empty;
        }

        var client = new SendGridClient(_apiKey);
        var data = new
        {
            list_ids = new[] { _contactList },
            contacts = new[]
            {
                new
                {
                    email = email,
                    first_name = firstName,
                    last_name = lastName
                }
            }
        };

        var response = await client.RequestAsync(
            method: SendGridClient.Method.PUT,
            urlPath: "marketing/contacts",
            requestBody: Newtonsoft.Json.JsonConvert.SerializeObject(data)
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to add contact to list: {response.StatusCode}");
        }
    }

    public static async Task SendWelcomeEmail(string toEmail, string toName)
    {
        string htmlFilePath = "https://searchaidirectory.com/html/EmailWelcome.html";

        var subject = "Welcome to SearchAiDirectory.com";

        var htmlContent = await GetHtmlContentFromServer(htmlFilePath);
        htmlContent = htmlContent.Replace("{{email}}", toEmail).Replace("{{name}}", toName);

        var plainTextContent = RegexHelper.HtmlToCleanText(htmlContent);

        await SendEmailAsync(toEmail, toName, subject, plainTextContent, htmlContent);
    }

    public static async Task SendPasswordResetEmail(string toEmail, string toName, string code)
    {
        string htmlFilePath = "https://searchaidirectory.com/html/EmailPasswordReset.html";

        var subject = "Reset Password to SearchAiDirectory.com";

        var htmlContent = await GetHtmlContentFromServer(htmlFilePath);
        htmlContent = htmlContent.Replace("{{email}}", toEmail).Replace("{{name}}", toName).Replace("{{code}}", code);

        var plainTextContent = RegexHelper.HtmlToCleanText(htmlContent);

        await SendEmailAsync(toEmail, toName, subject, plainTextContent, htmlContent);
    }

    public static async Task SendPasswordChangedEmail(string toEmail, string toName)
    {
        string htmlFilePath = "https://searchaidirectory.com/html/EmailPasswordChanged.html";

        var subject = "Password Changed to SearchAiDirectory.com";

        var htmlContent = await GetHtmlContentFromServer(htmlFilePath);
        htmlContent = htmlContent.Replace("{{email}}", toEmail).Replace("{{name}}", toName);

        var plainTextContent = RegexHelper.HtmlToCleanText(htmlContent);

        await SendEmailAsync(toEmail, toName, subject, plainTextContent, htmlContent);
    }

    public static async Task SendFormSubmissionEmail(string name, string email, string message)
    {
        var ourEmail = new EmailAddress("admin@searchaidirectory.com", "SearchAiDirectory");
        var subject = "Contact Form Submission Notification";
        var content = $"Name: {name}\nEmail: {email}\nMessage: {message}";
        var msg = MailHelper.CreateSingleEmail(ourEmail, ourEmail, subject, content, content);

        await new SendGridClient(_apiKey).SendEmailAsync(msg);
    }
}
