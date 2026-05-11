using global::Avalonia;
using global::Avalonia.Controls;
using Squiggle.UI.Avalonia.Helpers;

namespace Squiggle.UI.Avalonia.Controls;

public class MarkdownTextBlock : SelectableTextBlock
{
    public static readonly StyledProperty<string?> MarkdownProperty =
        AvaloniaProperty.Register<MarkdownTextBlock, string?>(nameof(Markdown));

    public string? Markdown
    {
        get => GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    static MarkdownTextBlock()
    {
        MarkdownProperty.Changed.AddClassHandler<MarkdownTextBlock>((tb, _) =>
        {
            tb.UpdateInlines();
        });
    }

    private void UpdateInlines()
    {
        Inlines?.Clear();

        if (string.IsNullOrEmpty(Markdown))
            return;

        var inlines = MarkdownParser.Parse(Markdown);
        if (Inlines is null)
            return;

        foreach (var inline in inlines)
            Inlines.Add(inline);
    }
}
