namespace CodexQuotaWidget.Core;

public enum MinimalSecondaryKind
{
    None,
    OtherQuota,
    ResetCredit
}

public sealed record MinimalSecondaryDisplay(
    MinimalSecondaryKind Kind,
    QuotaValue? Quota,
    DateTimeOffset? ResetCreditExpiresAt,
    DateTimeOffset? SelectedQuotaResetsAt)
{
    public static MinimalSecondaryDisplay None { get; } =
        new(MinimalSecondaryKind.None, null, null, null);
}

public static class MinimalSecondaryDisplayPolicy
{
    public static readonly TimeSpan ResetCreditPriorityWindow = TimeSpan.FromDays(3);

    public static MinimalSecondaryDisplay Select(
        QuotaPeriod selectedPeriod,
        QuotaSnapshot snapshot,
        int? resetCreditCount,
        DateTimeOffset? resetCreditExpiresAt,
        DateTimeOffset now)
    {
        // The compact row is context for the quota on the left. Keep that quota's
        // reset time even while the right side shows the other quota.
        var selectedQuotaResetsAt = snapshot.Get(selectedPeriod).ResetsAt;

        if (HasUrgentResetCredit(resetCreditCount, resetCreditExpiresAt, now))
        {
            return new MinimalSecondaryDisplay(
                MinimalSecondaryKind.ResetCredit,
                null,
                resetCreditExpiresAt,
                selectedQuotaResetsAt);
        }

        var otherPeriod = selectedPeriod == QuotaPeriod.FiveHours
            ? QuotaPeriod.Week
            : QuotaPeriod.FiveHours;
        var otherQuota = snapshot.Get(otherPeriod);
        if (otherQuota.IsAvailable && otherQuota.RemainingPercent is not null)
        {
            return new MinimalSecondaryDisplay(
                MinimalSecondaryKind.OtherQuota,
                otherQuota,
                null,
                selectedQuotaResetsAt);
        }

        // When Codex only returns one window, a known reset card is still more useful
        // than leaving the right side blank, even when it is not yet urgent.
        if (resetCreditCount is > 0 && resetCreditExpiresAt is DateTimeOffset expiresAt && expiresAt > now)
        {
            return new MinimalSecondaryDisplay(
                MinimalSecondaryKind.ResetCredit,
                null,
                expiresAt,
                selectedQuotaResetsAt);
        }

        return MinimalSecondaryDisplay.None;
    }

    private static bool HasUrgentResetCredit(
        int? resetCreditCount,
        DateTimeOffset? resetCreditExpiresAt,
        DateTimeOffset now) =>
        resetCreditCount is > 0 &&
        resetCreditExpiresAt is DateTimeOffset expiresAt &&
        expiresAt > now &&
        expiresAt - now <= ResetCreditPriorityWindow;
}
