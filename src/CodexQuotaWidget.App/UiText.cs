using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.App;

internal static class UiText
{
    public static string For(UiLanguage language, string chinese, string english) =>
        language == UiLanguage.English ? english : chinese;

    public static string Theme(WidgetTheme theme, UiLanguage language) => theme switch
    {
        WidgetTheme.Midnight => For(language, "深夜蓝", "Midnight"),
        WidgetTheme.Graphite => For(language, "石墨黑", "Graphite"),
        WidgetTheme.Paper => For(language, "纸张白", "Paper"),
        WidgetTheme.Aurora => For(language, "极光绿", "Aurora"),
        _ => theme.ToString()
    };

    public static string ResetCardCountdown(UiLanguage language, DateTimeOffset now, DateTimeOffset expiresAt)
    {
        if (language == UiLanguage.Chinese)
        {
            return ResetCreditCountdown.Format(now, expiresAt);
        }

        var remaining = expiresAt - now;
        if (remaining <= TimeSpan.Zero)
        {
            return "expired";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
        }

        if (remaining.TotalHours >= 1)
        {
            return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
        }

        return $"{Math.Max(1, remaining.Minutes)}m";
    }

    public static string QuotaResetCountdown(UiLanguage language, DateTimeOffset now, DateTimeOffset resetsAt) =>
        ResetCardCountdown(language, now, resetsAt);
}
