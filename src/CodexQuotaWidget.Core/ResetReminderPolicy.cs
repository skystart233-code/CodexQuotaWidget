namespace CodexQuotaWidget.Core;

public enum ResetReminderLevel
{
    None,
    Within24Hours,
    Within6Hours,
    Within1Hour,
    Expired
}

public static class ResetReminderPolicy
{
    public static ResetReminderLevel Evaluate(
        DateTimeOffset now,
        DateTimeOffset? expiresAt,
        int? availableCount,
        bool enabled)
    {
        if (!enabled || expiresAt is null || availableCount is null or <= 0)
        {
            return ResetReminderLevel.None;
        }

        var remaining = expiresAt.Value - now;
        if (remaining <= TimeSpan.Zero)
        {
            return ResetReminderLevel.Expired;
        }

        if (remaining <= TimeSpan.FromHours(1))
        {
            return ResetReminderLevel.Within1Hour;
        }

        if (remaining <= TimeSpan.FromHours(6))
        {
            return ResetReminderLevel.Within6Hours;
        }

        return remaining <= TimeSpan.FromHours(24)
            ? ResetReminderLevel.Within24Hours
            : ResetReminderLevel.None;
    }
}
