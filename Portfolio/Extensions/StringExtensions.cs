#region Imports

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Portfolio.Extensions;

//Marothi Mahlake - https://www.c-sharpcorner.com/blogs/make-url-slugs-in-asp-net-using-c-sharp2
public static class StringExtensions
{
    public static string RemoveHtmlTags(this string html)
    {
        var rx = new Regex("<[^>]*>");

        var noHtmlString = rx.Replace(html, "");

        return noHtmlString;
    }

    public static string Slugify(this string phrase)
    {
        var output = phrase.RemoveAccents().ToLower();

        output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");
        output = Regex.Replace(output, @"\s+", " ").Trim();
        output = Regex.Replace(output, @"\s", "-");

        return output;
    }

    private static string RemoveAccents(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = text.Normalize(NormalizationForm.FormD);
        var chars = text
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                        != UnicodeCategory.NonSpacingMark).ToArray();

        return new string(chars).Normalize(NormalizationForm.FormC);
    }
}