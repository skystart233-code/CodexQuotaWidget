using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CodexQuotaWidget.Core;
using MediaColor = System.Windows.Media.Color;
using WpfButton = System.Windows.Controls.Button;

namespace CodexQuotaWidget.App;

public partial class MainWindow : Window
{
    private QuotaPeriod _selectedPeriod;
    private bool _isMinimal;
    private WidgetTheme _theme;
    private DateTimeOffset? _resetCreditExpiresAt;
    private DateTimeOffset? _resetCreditIssuedAt;
    private bool _resetReminderEnabled;
    private int? _resetCreditCount;
    private bool _hasResetCreditDetails;
    private bool _statusIsError;
    private bool _weekIsCritical;
    private double? _selectedRemaining;
    private readonly DispatcherTimer _countdownTimer;

    public MainWindow(
        QuotaPeriod selectedPeriod,
        bool isMinimal,
        WidgetTheme theme,
        DateTimeOffset? resetCreditExpiresAt,
        bool resetReminderEnabled)
    {
        InitializeComponent();
        _countdownTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _countdownTimer.Tick += (_, _) => UpdateResetCardText();
        _countdownTimer.Start();
        SetSelectedPeriod(selectedPeriod);
        SetResetReminder(resetCreditExpiresAt, resetReminderEnabled);
        ApplyTheme(theme);
        SetMinimalMode(isMinimal);
        LocationChanged += (_, _) => PositionChanged?.Invoke(Left, Top);
        SizeChanged += (_, _) => Dispatcher.BeginInvoke(DispatcherPriority.Loaded, ClampToVisibleArea);
        Closing += (_, eventArgs) =>
        {
            if (AllowClose)
            {
                return;
            }

            eventArgs.Cancel = true;
            Hide();
        };
        Closed += (_, _) => _countdownTimer.Stop();
    }

    public bool AllowClose { get; set; }

    public event Action? RefreshRequested;
    public event Action<QuotaPeriod>? PeriodSelected;
    public event Action<bool>? MinimalModeSelected;
    public event Action<WidgetTheme>? ThemeSelected;
    public event Action? TrayEmojiRequested;
    public event Action<bool>? ResetReminderToggled;
    public event Action? ExitRequested;
    public event Action<double, double>? PositionChanged;

    public void SetSelectedPeriod(QuotaPeriod period)
    {
        _selectedPeriod = period;
        var label = period == QuotaPeriod.FiveHours ? "5H" : "周";
        PeriodText.Text = period == QuotaPeriod.FiveHours ? "5H 剩余" : "周额度剩余";
        MiniPeriodText.Text = label;
    }

    public void SetMinimalMode(bool isMinimal)
    {
        _isMinimal = isMinimal;
        MinimalPanel.Visibility = isMinimal ? Visibility.Visible : Visibility.Collapsed;
        ExpandedPanel.Visibility = isMinimal ? Visibility.Collapsed : Visibility.Visible;
        RootBorder.Padding = isMinimal ? new Thickness(10, 6, 10, 6) : new Thickness(13, 10, 13, 10);
        RootBorder.CornerRadius = new CornerRadius(isMinimal ? 13 : 16);
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            UpdateLayout();
            ClampToVisibleArea();
        });
    }

    public void ApplyTheme(WidgetTheme theme)
    {
        _theme = theme;
        ApplyVisualState();
    }

    public void SetResetReminder(DateTimeOffset? expiresAt, bool enabled)
    {
        _resetCreditExpiresAt = expiresAt;
        _resetReminderEnabled = enabled;
        UpdateResetCardText();
    }

    public void SetResetCredits(IReadOnlyList<ResetCredit> credits, bool reminderEnabled)
    {
        var now = DateTimeOffset.Now;
        var available = credits
            .Where(credit => credit.ExpiresAt > now)
            .OrderBy(credit => credit.ExpiresAt)
            .ToArray();
        var nearest = available.FirstOrDefault();
        _hasResetCreditDetails = true;
        _resetCreditCount = available.Length;
        _resetCreditIssuedAt = nearest?.IssuedAt;
        _resetCreditExpiresAt = nearest?.ExpiresAt;
        _resetReminderEnabled = reminderEnabled;
        UpdateResetCardText();
    }

    public void SetResetReminderEnabled(bool enabled)
    {
        _resetReminderEnabled = enabled;
        UpdateResetCardText();
    }

    public void RenderSnapshot(QuotaSnapshot snapshot, QuotaPeriod period)
    {
        if (!_hasResetCreditDetails)
        {
            _resetCreditCount = snapshot.ResetCreditCount;
        }
        _weekIsCritical = WeeklyQuotaAlertPolicy.IsCritical(snapshot.Week);
        SetSelectedPeriod(period);
        var value = snapshot.Get(period);
        if (value.IsAvailable && value.RemainingPercent is double remaining)
        {
            _selectedRemaining = remaining;
            ValueText.Text = $"{remaining:0.#}%";
            MiniValueText.Text = $"{remaining:0.#}%";
            AvailabilityText.Text = value.ResetsAt is DateTimeOffset reset
                ? $"重置 {reset.ToLocalTime():MM-dd HH:mm}"
                : $"{value.WindowDurationMins} 分钟窗口";
        }
        else
        {
            _selectedRemaining = null;
            ValueText.Text = "--";
            MiniValueText.Text = "--";
            AvailabilityText.Text = "此额度窗口不可用";
        }

        UpdateResetCardText();
        ApplyVisualState();
    }

    public void SetStatus(string text, bool isError)
    {
        _statusIsError = isError;
        StatusText.Text = text;
        ApplyVisualState();
    }

    private void UpdateResetCardText()
    {
        ResetCardText.Text = _resetCreditCount is int count ? $"重置卡 {count} 张" : "重置卡 --";
        var showCountdown = _resetCreditCount is > 0;
        MiniDivider.Visibility = showCountdown ? Visibility.Visible : Visibility.Collapsed;
        MiniResetCountdownText.Visibility = showCountdown ? Visibility.Visible : Visibility.Collapsed;
        if (_resetCreditExpiresAt is DateTimeOffset expiresAt)
        {
            var countdown = ResetCreditCountdown.Format(DateTimeOffset.Now, expiresAt);
            ResetExpiryText.Text = $"最近 {countdown}";
            MiniResetCountdownText.Text = $"卡 {countdown}";
            var localExpiry = expiresAt.ToLocalTime();
            var issued = _resetCreditIssuedAt is DateTimeOffset issuedAt
                ? $"\n发放：{issuedAt.ToLocalTime():MM-dd HH:mm}"
                : string.Empty;
            MiniResetCountdownText.ToolTip = $"最早到期：{localExpiry:MM-dd HH:mm}{issued}";
            ResetCardPanel.ToolTip = $"最早到期：{localExpiry:yyyy-MM-dd HH:mm}{issued}\n通知：{(_resetReminderEnabled ? "已开启" : "已关闭")}（右键切换）";
        }
        else
        {
            ResetExpiryText.Text = _resetCreditCount is 0 ? "暂无可用卡" : "正在查询到期时间";
            MiniResetCountdownText.Text = "卡 --";
            MiniResetCountdownText.ToolTip = null;
        }
    }

    private void ApplyVisualState()
    {
        var palette = ThemePalette.For(_theme);
        if (_weekIsCritical)
        {
            var primary = Brush("#FFFFFFFF");
            var secondary = Brush("#EFFFFFFF");
            RootBorder.Background = Brush("#F0C92D3A");
            RootBorder.BorderBrush = Brush("#FFFF7A84");
            ResetCardPanel.Background = Brush("#25FFFFFF");
            BrandText.Foreground = secondary;
            PeriodText.Foreground = primary;
            MiniPeriodText.Foreground = secondary;
            MiniValueText.Foreground = primary;
            MiniDivider.Foreground = Brush("#78FFFFFF");
            MiniResetCountdownText.Foreground = secondary;
            ValueText.Foreground = primary;
            AvailabilityText.Foreground = secondary;
            ResetCardText.Foreground = secondary;
            ResetExpiryText.Foreground = secondary;
            StatusText.Foreground = secondary;
            StatusDot.Fill = primary;
            MiniStatusDot.Fill = primary;
            foreach (var button in FindVisualChildren<WpfButton>(ExpandedPanel))
            {
                button.Foreground = secondary;
            }
            return;
        }

        RootBorder.Background = Brush(palette.Background);
        RootBorder.BorderBrush = Brush(palette.Border);
        BrandText.Foreground = Brush(palette.Muted);
        PeriodText.Foreground = Brush(palette.Primary);
        MiniPeriodText.Foreground = Brush(palette.Secondary);
        MiniDivider.Foreground = Brush(palette.Border);
        MiniResetCountdownText.Foreground = Brush(palette.Secondary);
        AvailabilityText.Foreground = Brush(palette.Muted);
        ResetCardPanel.Background = Brush(palette.Panel);
        ResetCardText.Foreground = Brush(palette.Secondary);
        ResetExpiryText.Foreground = Brush(palette.Muted);
        StatusText.Foreground = Brush(palette.Muted);
        var valueColor = Brush(_selectedRemaining is <= 15d ? palette.Danger : palette.Primary);
        ValueText.Foreground = valueColor;
        MiniValueText.Foreground = valueColor;
        var statusColor = Brush(_statusIsError ? palette.Danger : palette.Accent);
        StatusDot.Fill = statusColor;
        MiniStatusDot.Fill = statusColor;
        foreach (var button in FindVisualChildren<WpfButton>(ExpandedPanel))
        {
            button.Foreground = Brush(palette.Secondary);
        }
    }

    private ContextMenu CreateSettingsMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(CreateCheckItem("极简模式", _isMinimal, () => MinimalModeSelected?.Invoke(!_isMinimal)));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateCheckItem("显示 5H 剩余", _selectedPeriod == QuotaPeriod.FiveHours,
            () => PeriodSelected?.Invoke(QuotaPeriod.FiveHours)));
        menu.Items.Add(CreateCheckItem("显示周额度剩余", _selectedPeriod == QuotaPeriod.Week,
            () => PeriodSelected?.Invoke(QuotaPeriod.Week)));

        var themeMenu = new MenuItem { Header = "主题" };
        foreach (var theme in Enum.GetValues<WidgetTheme>())
        {
            var captured = theme;
            themeMenu.Items.Add(CreateCheckItem(ThemeLabel(theme), _theme == theme,
                () => ThemeSelected?.Invoke(captured)));
        }
        menu.Items.Add(themeMenu);
        menu.Items.Add(new Separator());
        var trayEmoji = new MenuItem { Header = "自定义托盘 Emoji…" };
        trayEmoji.Click += (_, _) => TrayEmojiRequested?.Invoke();
        menu.Items.Add(trayEmoji);
        menu.Items.Add(CreateCheckItem(
            "重置卡到期通知",
            _resetReminderEnabled,
            () => ResetReminderToggled?.Invoke(!_resetReminderEnabled)));
        return menu;
    }

    private static MenuItem CreateCheckItem(string label, bool isChecked, Action action)
    {
        var item = new MenuItem { Header = label, IsCheckable = true, IsChecked = isChecked };
        item.Click += (_, _) => action();
        return item;
    }

    private static string ThemeLabel(WidgetTheme theme) => theme switch
    {
        WidgetTheme.Midnight => "深夜蓝",
        WidgetTheme.Graphite => "石墨黑",
        WidgetTheme.Paper => "纸张白",
        WidgetTheme.Aurora => "极光绿",
        _ => theme.ToString()
    };

    private void ClampToVisibleArea()
    {
        if (double.IsNaN(Left) || double.IsNaN(Top))
        {
            return;
        }

        Left = Math.Clamp(Left, SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - ActualWidth);
        Top = Math.Clamp(Top, SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - ActualHeight);
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e) => RefreshRequested?.Invoke();
    private void MinimalButton_OnClick(object sender, RoutedEventArgs e) => MinimalModeSelected?.Invoke(true);
    private void ExitButton_OnClick(object sender, RoutedEventArgs e) => ExitRequested?.Invoke();

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var menu = CreateSettingsMenu();
        menu.PlacementTarget = (WpfButton)sender;
        menu.IsOpen = true;
    }

    private void RootBorder_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = CreateSettingsMenu();
        menu.PlacementTarget = RootBorder;
        menu.IsOpen = true;
        e.Handled = true;
    }

    private void DragSurface_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            MinimalModeSelected?.Invoke(!_isMinimal);
            e.Handled = true;
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static SolidColorBrush Brush(string value) =>
        new((MediaColor)System.Windows.Media.ColorConverter.ConvertFromString(value));

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }
            foreach (var nested in FindVisualChildren<T>(child))
            {
                yield return nested;
            }
        }
    }

    private sealed record ThemePalette(
        string Background,
        string Border,
        string Panel,
        string Primary,
        string Secondary,
        string Muted,
        string Accent,
        string Danger)
    {
        public static ThemePalette For(WidgetTheme theme) => theme switch
        {
            WidgetTheme.Graphite => new("#F01C1C1E", "#5058585E", "#402C2C2E", "#FFF4F4F5", "#FFD4D4D8", "#FF92929A", "#FF60A5FA", "#FFFF7373"),
            WidgetTheme.Paper => new("#F5F7F4EE", "#554C566A", "#305B6472", "#FF202632", "#FF3F4A5A", "#FF687386", "#FF2F80ED", "#FFD33F49"),
            WidgetTheme.Aurora => new("#F0132424", "#5067D6B8", "#353C806E", "#FFF0FFF9", "#FFC6F4E3", "#FF83B8A7", "#FF52E0B6", "#FFFF7F7F"),
            _ => new("#F01A202C", "#3FFFFFFF", "#302E3D54", "#FFF7FBFF", "#FFE4EAF1", "#FF91A3B8", "#FF57D699", "#FFFF7575")
        };
    }
}
