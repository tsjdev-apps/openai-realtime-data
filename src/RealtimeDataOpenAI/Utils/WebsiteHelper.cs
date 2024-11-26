using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace RealtimeDataOpenAI.Utils;

internal static partial class WebsiteHelper
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"(\r?\n)+")]
    private static partial Regex ExtraLineBreakRegex();


    /// <summary>
    ///    Gets the HTML body of a website as a string.
    /// </summary>
    /// <param name="url">The website.</param>
    /// <returns>Cleaned HTML body content</returns>
    public static async Task<string?> GetHtmlBodyAsync(string url)
    {
        try
        {
            // GET THE WEBSITE CONTENT AS STRING
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            // LOAD THE HTML DOCUMENT
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // GET THE BODY NODE
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

            // RETURN THE CLEANED HTML BODY
            return CleanHtmlBody(bodyNode?.InnerText);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteToConsole(
                $"[red]An error occurred: {ex.Message}[/]");
            return null;
        }
    }

    /// <summary>
    ///   Cleans the HTML body by removing extra whitespace and newlines.
    /// </summary>
    /// <param name="htmlBody">The HTML Body we want to clean.</param>
    /// <returns>Clean HTML body</returns>
    private static string? CleanHtmlBody(string? htmlBody)
    {
        // CHECK IF THE HTML BODY IS NULL OR EMPTY
        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            return htmlBody;
        }

        // REMOVE EXTRA WHITESPACE
        htmlBody = WhiteSpaceRegex().Replace(htmlBody, " ");

        // REMOVE EXTRA LINE BREAKS
        htmlBody = ExtraLineBreakRegex().Replace(htmlBody, "\n");

        // TRIM THE HTML BODY
        return htmlBody.Trim();
    }
}
