using System.Text.Json;

namespace CodexQuotaWidget.Core;

public static class RateLimitsMapper
{
    private const int FiveHoursMins = 300;
    private const int WeekMins = 10_080;

    public static QuotaSnapshot Map(JsonElement payload, DateTimeOffset? syncedAt = null)
    {
        var fiveHours = QuotaValue.Unavailable(QuotaPeriod.FiveHours);
        var week = QuotaValue.Unavailable(QuotaPeriod.Week);

        var rateLimits = UnwrapRateLimits(payload);
        if (rateLimits.ValueKind == JsonValueKind.Object)
        {
            // The app-server has historically named these primary/secondary, but the
            // useful contract is the window duration. Enumerating every window keeps
            // both 5H and weekly quotas working if Codex changes their ordering or names.
            foreach (var property in rateLimits.EnumerateObject())
            {
                var window = property.Value;
                if (window.ValueKind != JsonValueKind.Object ||
                    !TryReadInt(window, "windowDurationMins", out var duration))
                {
                    continue;
                }

                var period = Classify(duration);
                if (period is null)
                {
                    continue;
                }

                double? used = TryReadDouble(window, "usedPercent", out var usedValue)
                    ? Math.Clamp(usedValue, 0d, 100d)
                    : null;
                var value = new QuotaValue(
                    period.Value,
                    used.HasValue,
                    used,
                    used.HasValue ? 100 - used.Value : null,
                    duration,
                    TryReadTimestamp(window, "resetsAt"));

                if (period == QuotaPeriod.FiveHours)
                {
                    fiveHours = PreferAvailable(fiveHours, value);
                }
                else
                {
                    week = PreferAvailable(week, value);
                }
            }
        }

        return new QuotaSnapshot(
            fiveHours,
            week,
            ReadResetCreditCount(payload),
            syncedAt ?? DateTimeOffset.Now);
    }

    public static QuotaPeriod? Classify(int windowDurationMins)
    {
        if (Math.Abs(windowDurationMins - FiveHoursMins) <= 60)
        {
            return QuotaPeriod.FiveHours;
        }

        if (Math.Abs(windowDurationMins - WeekMins) <= 1_080)
        {
            return QuotaPeriod.Week;
        }

        return null;
    }

    private static JsonElement UnwrapRateLimits(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("rateLimits", out var rateLimits))
        {
            return rateLimits;
        }

        return payload;
    }

    private static QuotaValue PreferAvailable(QuotaValue current, QuotaValue candidate) =>
        !current.IsAvailable && candidate.IsAvailable ? candidate : current;

    private static bool TryReadInt(JsonElement element, string propertyName, out int value)
    {
        value = default;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String &&
               int.TryParse(property.GetString(), out value);
    }

    private static bool TryReadDouble(JsonElement element, string propertyName, out double value)
    {
        value = default;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String &&
               double.TryParse(
                   property.GetString(),
                   System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out value);
    }

    private static DateTimeOffset? TryReadTimestamp(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var unix))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix);
        }

        return property.ValueKind == JsonValueKind.String &&
               DateTimeOffset.TryParse(property.GetString(), out var timestamp)
            ? timestamp
            : null;
    }

    private static int? ReadResetCreditCount(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty("rateLimitResetCredits", out var credits) ||
            credits.ValueKind != JsonValueKind.Object ||
            !TryReadInt(credits, "availableCount", out var count))
        {
            return null;
        }

        return Math.Max(0, count);
    }
}
