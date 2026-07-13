using System.IO;
using System.Text.Json;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.App;

public enum WidgetTheme
{
    Midnight,
    Graphite,
    Paper,
    Aurora
}

public sealed class WidgetSettings
{
    public QuotaPeriod SelectedPeriod { get; set; } = QuotaPeriod.Week;
    public bool IsMinimal { get; set; } = true;
    public WidgetTheme Theme { get; set; } = WidgetTheme.Midnight;
    public string TrayEmoji { get; set; } = TrayEmojiValue.Default;
    public DateTimeOffset? ResetCreditExpiresAt { get; set; }
    public bool ResetCreditReminderEnabled { get; set; } = true;
    public string? LastResetReminderKey { get; set; }
    public double? Left { get; set; }
    public double? Top { get; set; }
}

public sealed class SettingsStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CodexQuotaWidget",
        "settings.json");

    public WidgetSettings Load()
    {
        try
        {
            var settings = File.Exists(_path)
                ? JsonSerializer.Deserialize<WidgetSettings>(File.ReadAllText(_path)) ?? new WidgetSettings()
                : new WidgetSettings();
            if (!Enum.IsDefined(settings.SelectedPeriod))
            {
                settings.SelectedPeriod = QuotaPeriod.Week;
            }
            if (!Enum.IsDefined(settings.Theme))
            {
                settings.Theme = WidgetTheme.Midnight;
            }
            if (!TrayEmojiValue.TryNormalize(settings.TrayEmoji, out var trayEmoji))
            {
                trayEmoji = TrayEmojiValue.Default;
            }
            settings.TrayEmoji = trayEmoji;

            return settings;
        }
        catch (JsonException)
        {
            return new WidgetSettings();
        }
        catch (IOException)
        {
            return new WidgetSettings();
        }
    }

    public void Save(WidgetSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
        catch (IOException)
        {
            // A transient settings write failure should not terminate the widget.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
