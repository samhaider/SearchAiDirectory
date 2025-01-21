namespace SearchAiDirectory.Shared.ThirdPartyService;

public static class AbstractEmailApi
{
    private static readonly HttpClient httpClient = new();
    private const string ApiKey = "cab0962ecb88471a94565ad98844822e";
    private const string BaseUrl = "https://emailvalidation.abstractapi.com/v1/";

    public static async Task<bool> ValidateEmail(string email)
    {
        var url = $"{BaseUrl}?api_key={ApiKey}&email={email}";
        var apiResponse = await httpClient.GetAsync(url);

        if (!apiResponse.IsSuccessStatusCode) return false;

        try
        {
            var response = JsonSerializer.Deserialize<AbstractEmailValidationResult>(await apiResponse.Content.ReadAsStringAsync());

            if (response == null || response.is_valid_format == null || response.is_mx_found == null || response.is_smtp_valid == null)
                return false;

            // Validation logic
            return response.deliverability == "DELIVERABLE" &&
                   response.is_valid_format.value &&
                   response.is_mx_found.value &&
                   response.is_smtp_valid.value &&
                   !response.is_disposable_email.value;
        }
        catch
        {
            // Return false in case of an exception
            return false;
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    public class AbstractEmailValidationResult
    {
        public string email { get; set; }
        public string autocorrect { get; set; }
        public string deliverability { get; set; }
        public string quality_score { get; set; }
        public AbstractEmailValidationIsValidFormat is_valid_format { get; set; }
        public AbstractEmailValidationIsFreeEmail is_free_email { get; set; }
        public AbstractEmailValidationIsDisposableEmail is_disposable_email { get; set; }
        public AbstractEmailValidationIsRoleEmail is_role_email { get; set; }
        public AbstractEmailValidationIsCatchallEmail is_catchall_email { get; set; }
        public AbstractEmailValidationIsMxFound is_mx_found { get; set; }
        public AbstractEmailValidationIsSmtpValid is_smtp_valid { get; set; }
    }

    public class AbstractEmailValidationIsValidFormat
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsFreeEmail
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsDisposableEmail
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsRoleEmail
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsCatchallEmail
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsMxFound
    {
        public bool value { get; set; }
        public string text { get; set; }
    }

    public class AbstractEmailValidationIsSmtpValid
    {
        public bool value { get; set; }
        public string text { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}