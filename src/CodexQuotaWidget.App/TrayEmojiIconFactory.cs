using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingIcon = System.Drawing.Icon;
using DrawingSystemIcons = System.Drawing.SystemIcons;

namespace CodexQuotaWidget.App;

internal static class TrayEmojiIconFactory
{
    public static DrawingIcon Create(string emoji)
    {
        try
        {
            const int size = 64;
            var surface = new Grid
            {
                Width = size,
                Height = size,
                Background = System.Windows.Media.Brushes.Transparent
            };
            surface.Children.Add(new Border
            {
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(18),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(235, 35, 43, 56))
            });
            surface.Children.Add(new TextBlock
            {
                Text = emoji,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI Emoji"),
                FontSize = 39,
                Foreground = System.Windows.Media.Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, -2, 0, 0)
            });

            surface.Measure(new System.Windows.Size(size, size));
            surface.Arrange(new Rect(0, 0, size, size));
            surface.UpdateLayout();
            var rendered = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rendered.Render(surface);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rendered));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Position = 0;
            using var bitmap = new DrawingBitmap(stream);
            var handle = bitmap.GetHicon();
            try
            {
                using var borrowed = DrawingIcon.FromHandle(handle);
                return (DrawingIcon)borrowed.Clone();
            }
            finally
            {
                DestroyIcon(handle);
            }
        }
        catch
        {
            return (DrawingIcon)DrawingSystemIcons.Application.Clone();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);
}
