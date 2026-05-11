using System.Collections.Generic;
using System.Text.RegularExpressions;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Media;

namespace Squiggle.UI.Avalonia.Helpers;

public static class MarkdownParser
{
    private static readonly Regex MarkdownPattern = new(
        @"(```[\s\S]*?```)|(`[^`]+`)|(\*\*[^*]+\*\*)|(~~[^~]+~~)|(\*[^*]+\*)|(\[([^\]]+)\]\(([^)]+)\))",
        RegexOptions.Compiled);

    private static readonly FontFamily MonoFont = new("Cascadia Code,Consolas,Courier New");

    public static IEnumerable<Inline> Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        int lastIndex = 0;
        foreach (Match match in MarkdownPattern.Matches(text))
        {
            if (match.Index > lastIndex)
            {
                yield return new Run(text[lastIndex..match.Index]);
            }

            if (match.Groups[1].Success)
            {
                // Code block: ```...```
                string code = match.Value[3..^3].Trim();
                yield return new Run(code) { FontFamily = MonoFont };
            }
            else if (match.Groups[2].Success)
            {
                // Inline code: `...`
                string code = match.Value[1..^1];
                yield return new Run(code) { FontFamily = MonoFont };
            }
            else if (match.Groups[3].Success)
            {
                // Bold: **...**
                string content = match.Value[2..^2];
                yield return new Run(content) { FontWeight = FontWeight.Bold };
            }
            else if (match.Groups[4].Success)
            {
                // Strikethrough: ~~...~~
                string content = match.Value[2..^2];
                yield return new Run(content) { TextDecorations = TextDecorations.Strikethrough };
            }
            else if (match.Groups[5].Success)
            {
                // Italic: *...*
                string content = match.Value[1..^1];
                yield return new Run(content) { FontStyle = FontStyle.Italic };
            }
            else if (match.Groups[6].Success)
            {
                // Link: [text](url)
                string linkText = match.Groups[7].Value;
                yield return new Run(linkText)
                {
                    TextDecorations = TextDecorations.Underline,
                    Foreground = Brushes.CornflowerBlue
                };
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            yield return new Run(text[lastIndex..]);
        }
    }
}
