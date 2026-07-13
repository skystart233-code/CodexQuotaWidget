namespace CodexQuotaWidget.Core;

public sealed record ResetCredit(
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);

public static class ResetCreditCountdown
{
    public static string Format(DateTimeOffset now, DateTimeOffset expiresAt)
    {
        var remaining = expiresAt - now;
        if (remaining <= TimeSpan.Zero)
        {
            return "已到期";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"{(int)remaining.TotalDays}天{remaining.Hours}时";
        }

        if (remaining.TotalHours >= 1)
        {
            return $"{(int)remaining.TotalHours}时{remaining.Minutes}分";
        }

        return $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes))}分";
    }
}

public static class WeeklyQuotaAlertPolicy
{
    public static bool IsCritical(QuotaValue week) =>
        week.IsAvailable && week.RemainingPercent is double remaining && remaining < 5d;
}
