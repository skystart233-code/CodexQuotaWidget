using System.Globalization;
using System.Text.Json;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.Codex;

public static class ResetCreditsMapper
{
    private static readonly HashSet<string> IssuedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "issued_at", "issuedAt", "created_at", "createdAt", "granted_at", "grantedAt",
        "grant_time", "grantTime", "start_time", "startTime", "available_at", "availableAt"
    };

    private static readonly HashSet<string> ExpiryKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "expires_at", "expiresAt", "expired_at", "expiredAt", "expiration_time", "expirationTime",
        "expire_time", "expireTime", "valid_until", "validUntil", "end_time", "endTime"
    };

    public static IReadOnlyList<ResetCredit> Map(JsonElement payload)
    {
        var credits = new List<ResetCredit>();
        Walk(payload, credits);
        return credits
            .DistinctBy(credit => (credit.IssuedAt.UtcTicks, credit.ExpiresAt.UtcTicks))
            .OrderBy(credit => credit.ExpiresAt)
            .ToArray();
    }

    private static void Walk(JsonElement node, ICollection<ResetCredit> credits)
    {
        if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in node.EnumerateArray())
            {
                Walk(item, credits);
            }
            return;
        }

        if (node.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        DateTimeOffset? issuedAt = null;
        DateTimeOffset? expiresAt = null;
        foreach (var property in node.EnumerateObject())
        {
            if (issuedAt is null && IssuedKeys.Contains(property.Name))
            {
                issuedAt = ParseTimestamp(property.Value);
            }
            if (expiresAt is null && ExpiryKeys.Contains(property.Name))
            {
                expiresAt = ParseTimestamp(property.Value);
            }
        }

        if (issuedAt is not null && expiresAt is not null)
        {
            credits.Add(new ResetCredit(issuedAt.Value, expiresAt.Value));
        }

        foreach (var property in node.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            {
                Walk(property.Value, credits);
            }
        }
    }

    private static DateTimeOffset? ParseTimestamp(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var unix))
        {
            try
            {
                return unix >= 1_000_000_000_000
                    ? DateTimeOffset.FromUnixTimeMilliseconds(unix)
                    : DateTimeOffset.FromUnixTimeSeconds(unix);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
        {
            try
            {
                return numeric >= 1_000_000_000_000
                    ? DateTimeOffset.FromUnixTimeMilliseconds(numeric)
                    : DateTimeOffset.FromUnixTimeSeconds(numeric);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        return DateTimeOffset.TryParse(
            text,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var timestamp)
            ? timestamp
            : null;
    }
}
