namespace CodexQuotaWidget.Core;

public enum QuotaPeriod
{
    FiveHours,
    Week
}

public sealed record QuotaValue(
    QuotaPeriod Period,
    bool IsAvailable,
    double? UsedPercent,
    double? RemainingPercent,
    int? WindowDurationMins,
    DateTimeOffset? ResetsAt)
{
    public static QuotaValue Unavailable(QuotaPeriod period) =>
        new(period, false, null, null, null, null);
}

public sealed record QuotaSnapshot(
    QuotaValue FiveHours,
    QuotaValue Week,
    int? ResetCreditCount,
    DateTimeOffset SyncedAt)
{
    public QuotaValue Get(QuotaPeriod period) =>
        period == QuotaPeriod.FiveHours ? FiveHours : Week;
}
