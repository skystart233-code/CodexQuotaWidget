namespace CodexQuotaWidget.Core;

public static class QuotaAlertPolicy
{
    public const double CriticalRemainingPercent = 5d;

    public static bool IsCritical(QuotaValue quota) =>
        quota.IsAvailable && quota.RemainingPercent is double remaining && remaining < CriticalRemainingPercent;

    public static bool IsAnyCritical(QuotaSnapshot snapshot) =>
        IsCritical(snapshot.FiveHours) || IsCritical(snapshot.Week);
}
