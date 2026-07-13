using System.Drawing;
using System.IO;
using System.Windows;
using CodexQuotaWidget.Codex;
using CodexQuotaWidget.Core;
using Forms = System.Windows.Forms;

namespace CodexQuotaWidget.App;

public partial class App : System.Windows.Application
{
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private readonly SettingsStore _settingsStore = new();
    private readonly ResetCreditsClient _resetCreditsClient = new();
    private MainWindow? _window;
    private WidgetSettings _settings = new();
    private Forms.NotifyIcon? _trayIcon;
    private Icon? _trayEmojiIcon;
    private CodexQuotaService? _service;
    private QuotaSnapshot? _snapshot;
    private IReadOnlyList<ResetCredit> _resetCredits = [];
    private DateTimeOffset? _lastResetCreditsAttemptAt;
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            _settings = _settingsStore.Load();
            _window = new MainWindow(
                _settings.SelectedPeriod,
                _settings.IsMinimal,
                _settings.Theme,
                _settings.ResetCreditExpiresAt,
                _settings.ResetCreditReminderEnabled);
            RestorePosition(_window, _settings);
            _window.RefreshRequested += () => _ = RefreshAsync(forceReconnect: false, forceResetCredits: true);
            _window.PeriodSelected += SelectPeriod;
            _window.MinimalModeSelected += SelectMinimalMode;
            _window.ThemeSelected += SelectTheme;
            _window.TrayEmojiRequested += ConfigureTrayEmoji;
            _window.ResetReminderToggled += SetResetReminderEnabled;
            _window.ExitRequested += () => _ = ShutdownAsync();
            _window.PositionChanged += SavePosition;
            _window.Show();

            CreateTrayIcon();
            _ = RefreshAsync(forceReconnect: false, forceResetCredits: true);
            _ = RunPeriodicRefreshAsync(_lifetime.Token);
        }
        catch (Exception exception)
        {
            WriteStartupFailure(exception);
            System.Windows.MessageBox.Show(
                "Codex 额度浮窗启动失败。诊断信息已写入本机日志。",
                "CodexQuotaWidget",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void WriteStartupFailure(Exception exception)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CodexQuotaWidget");
            Directory.CreateDirectory(directory);
            File.AppendAllText(
                Path.Combine(directory, "startup-errors.log"),
                $"[{DateTimeOffset.Now:O}] {exception}\n");
        }
        catch
        {
        }
    }

    private async Task RefreshAsync(bool forceReconnect, bool forceResetCredits)
    {
        if (_isExiting || _window is null)
        {
            return;
        }

        await _refreshGate.WaitAsync(_lifetime.Token).ConfigureAwait(false);
        try
        {
            await Dispatcher.InvokeAsync(() => _window.SetStatus("正在同步…", isError: false));
            if (forceReconnect || _service is null)
            {
                await DisposeServiceAsync().ConfigureAwait(false);
                var client = new CodexAppServerClient();
                _service = new CodexQuotaService(client);
                _service.SnapshotReceived += OnSnapshotReceived;
                await _service.StartAsync(_lifetime.Token).ConfigureAwait(false);
            }
            else
            {
                await _service.RefreshAsync(_lifetime.Token).ConfigureAwait(false);
            }

            await RefreshResetCreditsAsync(forceResetCredits).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            await DisposeServiceAsync().ConfigureAwait(false);
            var message = exception.InnerException?.Message ?? exception.Message;
            await Dispatcher.InvokeAsync(() => _window?.SetStatus(
                $"同步失败：{Shorten(message, 48)}",
                isError: true));
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private void OnSnapshotReceived(QuotaSnapshot snapshot)
    {
        _snapshot = snapshot;
        _ = Dispatcher.InvokeAsync(() =>
        {
            _window?.RenderSnapshot(snapshot, _settings.SelectedPeriod);
            _window?.SetStatus($"已同步 {snapshot.SyncedAt:HH:mm:ss}", isError: false);
            CheckResetReminder();
        });
    }

    private async Task RunPeriodicRefreshAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await RefreshAsync(forceReconnect: false, forceResetCredits: false).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void SelectPeriod(QuotaPeriod period)
    {
        _settings.SelectedPeriod = period;
        _window?.SetSelectedPeriod(period);
        if (_snapshot is not null)
        {
            _window?.RenderSnapshot(_snapshot, period);
        }
        _settingsStore.Save(_settings);
        UpdateTrayChecks();
    }

    private void SelectMinimalMode(bool isMinimal)
    {
        _settings.IsMinimal = isMinimal;
        _window?.SetMinimalMode(isMinimal);
        _settingsStore.Save(_settings);
        UpdateTrayChecks();
    }

    private void SelectTheme(WidgetTheme theme)
    {
        _settings.Theme = theme;
        _window?.ApplyTheme(theme);
        _settingsStore.Save(_settings);
        UpdateTrayChecks();
    }

    private void ConfigureTrayEmoji()
    {
        if (_window is null)
        {
            return;
        }

        var dialog = new TrayEmojiDialog(_settings.TrayEmoji, _settings.Theme);
        if (_window.IsVisible)
        {
            dialog.Owner = _window;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _settings.TrayEmoji = dialog.Emoji;
        _settingsStore.Save(_settings);
        ApplyTrayEmojiIcon();
    }

    private void ApplyTrayEmojiIcon()
    {
        if (_trayIcon is null)
        {
            return;
        }

        var next = TrayEmojiIconFactory.Create(_settings.TrayEmoji);
        _trayIcon.Icon = next;
        var previous = _trayEmojiIcon;
        _trayEmojiIcon = next;
        previous?.Dispose();
    }

    private void SetResetReminderEnabled(bool enabled)
    {
        _settings.ResetCreditReminderEnabled = enabled;
        _settings.LastResetReminderKey = null;
        _settingsStore.Save(_settings);
        _window?.SetResetReminderEnabled(enabled);
        UpdateTrayChecks();
        CheckResetReminder();
    }

    private async Task RefreshResetCreditsAsync(bool force)
    {
        var now = DateTimeOffset.Now;
        if (!force && _lastResetCreditsAttemptAt is DateTimeOffset last &&
            now - last < TimeSpan.FromMinutes(10))
        {
            return;
        }

        _lastResetCreditsAttemptAt = now;
        try
        {
            var credits = await _resetCreditsClient.ReadAsync(_lifetime.Token).ConfigureAwait(false);
            _resetCredits = credits;
            var nearest = credits
                .Where(credit => credit.ExpiresAt > now)
                .OrderBy(credit => credit.ExpiresAt)
                .FirstOrDefault();
            _settings.ResetCreditExpiresAt = nearest?.ExpiresAt;
            _settingsStore.Save(_settings);
            await Dispatcher.InvokeAsync(() =>
            {
                _window?.SetResetCredits(credits, _settings.ResetCreditReminderEnabled);
                CheckResetReminder();
            });
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            // Quota remains useful when the private reset-credit endpoint is temporarily
            // unavailable. Keep the last successful expiry cached and try again later.
        }
    }

    private void CheckResetReminder()
    {
        var count = _resetCredits.Count > 0
            ? _resetCredits.Count(credit => credit.ExpiresAt > DateTimeOffset.Now)
            : _snapshot?.ResetCreditCount;
        var level = ResetReminderPolicy.Evaluate(
            DateTimeOffset.Now,
            _settings.ResetCreditExpiresAt,
            count,
            _settings.ResetCreditReminderEnabled);
        if (level is ResetReminderLevel.None or ResetReminderLevel.Expired ||
            _settings.ResetCreditExpiresAt is not DateTimeOffset expiresAt ||
            count is not int availableCount)
        {
            return;
        }

        var key = $"{expiresAt.UtcTicks}:{level}";
        if (_settings.LastResetReminderKey == key)
        {
            return;
        }

        var remainingText = level switch
        {
            ResetReminderLevel.Within1Hour => "不足 1 小时",
            ResetReminderLevel.Within6Hours => "不足 6 小时",
            _ => "不足 24 小时"
        };
        _trayIcon?.ShowBalloonTip(
            8_000,
            "Codex 重置卡即将到期",
            $"最近一张将在 {expiresAt.ToLocalTime():MM-dd HH:mm} 到期（{remainingText}），当前共 {availableCount} 张可用。",
            Forms.ToolTipIcon.Warning);
        _settings.LastResetReminderKey = key;
        _settingsStore.Save(_settings);
    }

    private void CreateTrayIcon()
    {
        var menu = new Forms.ContextMenuStrip();
        var showItem = new Forms.ToolStripMenuItem("显示浮窗");
        showItem.Click += (_, _) => Dispatcher.Invoke(ShowWindow);
        var minimalItem = new Forms.ToolStripMenuItem("极简模式") { Name = "Minimal" };
        minimalItem.Click += (_, _) => Dispatcher.Invoke(() => SelectMinimalMode(!_settings.IsMinimal));
        var fiveHoursItem = new Forms.ToolStripMenuItem("显示 5H 剩余") { Name = "FiveHours" };
        fiveHoursItem.Click += (_, _) => Dispatcher.Invoke(() => SelectPeriod(QuotaPeriod.FiveHours));
        var weekItem = new Forms.ToolStripMenuItem("显示周额度剩余") { Name = "Week" };
        weekItem.Click += (_, _) => Dispatcher.Invoke(() => SelectPeriod(QuotaPeriod.Week));
        var refreshItem = new Forms.ToolStripMenuItem("立即刷新");
        refreshItem.Click += (_, _) => _ = RefreshAsync(forceReconnect: false, forceResetCredits: true);
        var themeMenu = new Forms.ToolStripMenuItem("主题") { Name = "Themes" };
        foreach (var theme in Enum.GetValues<WidgetTheme>())
        {
            var captured = theme;
            var item = new Forms.ToolStripMenuItem(ThemeLabel(theme)) { Name = $"Theme_{theme}" };
            item.Click += (_, _) => Dispatcher.Invoke(() => SelectTheme(captured));
            themeMenu.DropDownItems.Add(item);
        }
        var reminderItem = new Forms.ToolStripMenuItem("重置卡到期通知") { Name = "ResetReminder" };
        reminderItem.Click += (_, _) => Dispatcher.Invoke(
            () => SetResetReminderEnabled(!_settings.ResetCreditReminderEnabled));
        var emojiItem = new Forms.ToolStripMenuItem("自定义托盘 Emoji…");
        emojiItem.Click += (_, _) => Dispatcher.Invoke(ConfigureTrayEmoji);
        var exitItem = new Forms.ToolStripMenuItem("退出");
        exitItem.Click += (_, _) => Dispatcher.Invoke(() => _ = ShutdownAsync());
        menu.Items.AddRange([
            showItem,
            minimalItem,
            fiveHoursItem,
            weekItem,
            themeMenu,
            emojiItem,
            reminderItem,
            refreshItem,
            new Forms.ToolStripSeparator(),
            exitItem
        ]);

        _trayEmojiIcon = TrayEmojiIconFactory.Create(_settings.TrayEmoji);
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = _trayEmojiIcon,
            Text = "Codex 额度浮窗",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowWindow);
        UpdateTrayChecks();
    }

    private void UpdateTrayChecks()
    {
        if (_trayIcon?.ContextMenuStrip is not { } menu)
        {
            return;
        }

        if (menu.Items["FiveHours"] is Forms.ToolStripMenuItem fiveHours)
        {
            fiveHours.Checked = _settings.SelectedPeriod == QuotaPeriod.FiveHours;
        }
        if (menu.Items["Week"] is Forms.ToolStripMenuItem week)
        {
            week.Checked = _settings.SelectedPeriod == QuotaPeriod.Week;
        }
        if (menu.Items["Minimal"] is Forms.ToolStripMenuItem minimal)
        {
            minimal.Checked = _settings.IsMinimal;
        }
        if (menu.Items["ResetReminder"] is Forms.ToolStripMenuItem reminder)
        {
            reminder.Checked = _settings.ResetCreditReminderEnabled;
        }
        if (menu.Items["Themes"] is Forms.ToolStripMenuItem themes)
        {
            foreach (Forms.ToolStripItem raw in themes.DropDownItems)
            {
                if (raw is Forms.ToolStripMenuItem item)
                {
                    item.Checked = item.Name == $"Theme_{_settings.Theme}";
                }
            }
        }
    }

    private static string ThemeLabel(WidgetTheme theme) => theme switch
    {
        WidgetTheme.Midnight => "深夜蓝",
        WidgetTheme.Graphite => "石墨黑",
        WidgetTheme.Paper => "纸张白",
        WidgetTheme.Aurora => "极光绿",
        _ => theme.ToString()
    };

    private void ShowWindow()
    {
        if (_window is null)
        {
            return;
        }

        _window.Show();
        _window.Activate();
    }

    private void SavePosition(double left, double top)
    {
        _settings.Left = left;
        _settings.Top = top;
        _settingsStore.Save(_settings);
    }

    private static void RestorePosition(Window window, WidgetSettings settings)
    {
        if (settings.Left is double left && settings.Top is double top &&
            left >= SystemParameters.VirtualScreenLeft - 100 &&
            left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 40 &&
            top >= SystemParameters.VirtualScreenTop - 40 &&
            top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 40)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = left;
            window.Top = top;
        }
    }

    private async Task DisposeServiceAsync()
    {
        if (_service is null)
        {
            return;
        }

        var service = _service;
        _service = null;
        service.SnapshotReceived -= OnSnapshotReceived;
        await service.DisposeAsync().ConfigureAwait(false);
    }

    private async Task ShutdownAsync()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        _lifetime.Cancel();
        _trayIcon?.Dispose();
        _trayEmojiIcon?.Dispose();
        _resetCreditsClient.Dispose();
        await DisposeServiceAsync().ConfigureAwait(false);
        await Dispatcher.InvokeAsync(() =>
        {
            if (_window is not null)
            {
                _window.AllowClose = true;
            }

            Shutdown();
        });
    }

    private static string Shorten(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 1)] + "…";
}
