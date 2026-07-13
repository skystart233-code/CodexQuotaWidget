using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.App;

public partial class TrayEmojiDialog : Window
{
    public TrayEmojiDialog(string currentEmoji, WidgetTheme theme, UiLanguage language)
    {
        InitializeComponent();
        Title = UiText.For(language, "托盘 Emoji", "Tray Emoji");
        TitleText.Text = Title;
        HintText.Text = UiText.For(
            language,
            "输入一个 Emoji 或符号，保存后立即显示在托盘。",
            "Enter one emoji or symbol. It appears in the tray right away.");
        ErrorText.Text = UiText.For(language, "请只输入一个 Emoji 或符号", "Use one emoji or symbol only");
        CancelButton.Content = UiText.For(language, "取消", "Cancel");
        SaveButton.Content = UiText.For(language, "保存", "Save");
        EmojiTextBox.Text = currentEmoji;
        ApplyTheme(theme);
        Loaded += (_, _) =>
        {
            EmojiTextBox.Focus();
            EmojiTextBox.SelectAll();
        };
    }

    public string Emoji { get; private set; } = TrayEmojiValue.Default;

    private void ApplyTheme(WidgetTheme theme)
    {
        var colors = theme switch
        {
            WidgetTheme.Graphite => new[] { "#FF1C1C1E", "#FF58585E", "#FFF4F4F5", "#FF92929A", "#FF2C2C2E" },
            WidgetTheme.Paper => new[] { "#FFF7F4EE", "#FFB2B7C0", "#FF202632", "#FF687386", "#FFFFFFFF" },
            WidgetTheme.Aurora => new[] { "#FF132424", "#FF397062", "#FFF0FFF9", "#FF83B8A7", "#FF203B37" },
            _ => new[] { "#FF1A202C", "#FF435069", "#FFF7FBFF", "#FF91A3B8", "#FF252E3D" }
        };
        DialogBorder.Background = Brush(colors[0]);
        DialogBorder.BorderBrush = Brush(colors[1]);
        TitleText.Foreground = Brush(colors[2]);
        HintText.Foreground = Brush(colors[3]);
        ErrorText.Foreground = Brush("#FFFF7575");
        CloseButton.Foreground = Brush(colors[3]);
        EmojiTextBox.Foreground = Brush(colors[2]);
        EmojiTextBox.Background = Brush(colors[4]);
        EmojiTextBox.BorderBrush = Brush(colors[1]);
        CancelButton.Foreground = Brush(colors[2]);
        CancelButton.Background = Brush(colors[4]);
        foreach (var button in new[] { PresetOne, PresetTwo, PresetThree, PresetFour })
        {
            button.Foreground = Brush(colors[2]);
            button.Background = Brush(colors[4]);
        }
    }

    private void PresetButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: string emoji })
        {
            EmojiTextBox.Text = emoji;
            EmojiTextBox.CaretIndex = EmojiTextBox.Text.Length;
            ErrorText.Visibility = Visibility.Collapsed;
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TrayEmojiValue.TryNormalize(EmojiTextBox.Text, out var emoji))
        {
            ErrorText.Visibility = Visibility.Visible;
            EmojiTextBox.Focus();
            EmojiTextBox.SelectAll();
            return;
        }

        Emoji = emoji;
        DialogResult = true;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void DialogBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static SolidColorBrush Brush(string value) =>
        new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value));
}
