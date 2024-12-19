using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace SearchAiDirectory.ConsoleApp;

public static class WebScraper
{
    private static readonly string[] sitePages = ["about", "services", "products", "contact", "about us", "our services", "our products", "contact us", "about-us", "our-services", "our-products", "contact-us", "who we are", "what we do", "our team", "team", "team members", "projects", "portfolio"];

    public static async Task<string> Scrape(string url)
    {
        try
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            using var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);

            // Wait for the page to load completely
            await Task.Delay(5000);

            var finalUrl = driver.Url.Replace("?ref=futuretools.io", "");
            var htmlContent = driver.PageSource;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Extract title, meta description, and headings
            string title = ExtractTitle(htmlDoc);
            string metaDescription = ExtractMetaDescription(htmlDoc);
            var headings = ExtractHeadings(htmlDoc);

            string websiteData = "Website:" + finalUrl + "\n";

            // Display extracted main information
            websiteData += "Main Page Information:" +
                "\nTitle: " + title +
                "\nMeta Description: " + metaDescription +
                "Headings: " + string.Join(" - ", headings);

            var keyLinks = ExtractLinks(htmlDoc);

            // Visit each link one level deep and extract additional information
            foreach (var link in keyLinks)
            {
                string fullLink = link.StartsWith("http") ? link : new Uri(new Uri(finalUrl), link).ToString();
                websiteData += await GetAdditionalContent(driver, fullLink);
            }

            return websiteData;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return string.Empty;
        }
    }

    private static string ExtractTitle(HtmlDocument htmlDoc)
    {
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
        return titleNode != null ? titleNode.InnerText.Trim() : "No Title";
    }

    private static string ExtractMetaDescription(HtmlDocument htmlDoc)
    {
        var metaDescriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
        return metaDescriptionNode?.GetAttributeValue("content", "No Meta Description");
    }

    private static List<string> ExtractHeadings(HtmlDocument htmlDoc)
    {
        var headings = new List<string>();
        foreach (var headingTag in new[] { "h1", "h2", "h3", "h4", "h5" })
        {
            var nodes = htmlDoc.DocumentNode.SelectNodes($"//{headingTag}");

            if (nodes != null) headings.AddRange(nodes.Select(node => node.InnerText.Trim()));
        }
        return headings;
    }

    private static List<string> ExtractLinks(HtmlDocument htmlDoc)
    {
        var links = new List<string>();
        var anchorNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes is not null)
            foreach (var link in anchorNodes)
            {
                string linkText = link.InnerText.ToLower().Trim().Normalize();
                if (sitePages.Any(linkText.Contains))
                {
                    string href = link.GetAttributeValue("href", string.Empty);

                    if (!string.IsNullOrEmpty(href) && !links.Contains(href)) links.Add(href);
                }
            }

        return links;
    }

    private static async Task<string> GetAdditionalContent(ChromeDriver driver, string url)
    {
        try
        {
            driver.Navigate().GoToUrl(url);

            // Wait for the page to load completely
            await Task.Delay(5000);

            var htmlContent = driver.PageSource;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Extract title, headings, and detailed content blocks from the secondary page
            string title = ExtractTitle(htmlDoc);
            var headings = ExtractHeadings(htmlDoc);
            var contentBlocks = ExtractContentBlocks(htmlDoc);

            // Display additional content
            string additionalContent = $"Additional Content from {url}:" +
                "\nTitle: " + title +
                "\nHeadings: " + string.Join(" - ", headings) +
                "\nDetailed Content: " + string.Join(" - ", contentBlocks);

            return additionalContent;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static List<string> ExtractContentBlocks(HtmlDocument htmlDoc)
    {
        var contentBlocks = new List<string>();
        // Extract paragraphs that are meaningful, ignoring too-short or repetitive content
        var nodes = htmlDoc.DocumentNode.SelectNodes("//p");
        if (nodes != null)
            foreach (var node in nodes)
            {
                string text = node.InnerText.Trim();
                if (text.Length > 100) // Exclude too short paragraphs
                {
                    contentBlocks.Add(text);
                }
            }

        return contentBlocks;
    }



    public static async Task<List<string>> ExtractEmail(string url)
    {
        // Find email addresses on the website
        var emailAddresses = await FindEmailAddresses(url);
        return emailAddresses;
    }

    private static async Task<List<string>> FindEmailAddresses(string url, HashSet<string> visitedUrls = null)
    {
        visitedUrls ??= [];
        var emailAddresses = new List<string>();

        if (visitedUrls.Contains(url))
            return emailAddresses;

        visitedUrls.Add(url);

        try
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string htmlContent = await response.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Extract email addresses from the current page
                emailAddresses.AddRange(ExtractEmailAddresses(htmlDoc));

                // Extract links to visit
                var links = ExtractLinks(htmlDoc);

                // Recursively visit each link to find more email addresses
                foreach (var link in links)
                {
                    string fullLink = link.StartsWith("http") ? link : new Uri(new Uri(url), link).ToString();
                    emailAddresses.AddRange(await FindEmailAddresses(fullLink, visitedUrls));
                }
            }
        }
        catch
        {
            // Ignore errors and continue
        }

        return emailAddresses.Distinct().ToList();
    }

    private static List<string> ExtractEmailAddresses(HtmlDocument htmlDoc)
    {
        var emailAddresses = new List<string>();
        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);

        var textNodes = htmlDoc.DocumentNode.SelectNodes("//text()");
        if (textNodes != null)
        {
            foreach (var node in textNodes)
            {
                var matches = emailRegex.Matches(node.InnerText);
                foreach (Match match in matches)
                    emailAddresses.Add(match.Value);
            }
        }

        return emailAddresses;
    }
}

