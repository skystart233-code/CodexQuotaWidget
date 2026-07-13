using System.Globalization;

namespace CodexQuotaWidget.Core;

public static class TrayEmojiValue
{
    public const string Default = "⚡";

    public static bool TryNormalize(string? input, out string value)
    {
        value = Default;
        var text = input?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var elements = StringInfo.GetTextElementEnumerator(text);
        if (!elements.MoveNext())
        {
            return false;
        }

        var first = (string)elements.Current;
        if (elements.MoveNext())
        {
            return false;
        }

        value = first;
        return true;
    }
}
