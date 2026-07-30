using Microsoft.Win32;

namespace CodexQuotaWidget.App;

internal static class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "CodexQuotaWidget.CodexWatcher";

    public static bool TrySetEnabled(bool enabled, out string? error)
    {
        try
        {
            if (enabled)
            {
                var executable = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(executable))
                {
                    error = "The widget executable path could not be determined.";
                    return false;
                }

                using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
                key.SetValue(ValueName, CreateCommand(executable), RegistryValueKind.String);
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
                key?.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            error = null;
            return true;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or System.IO.IOException or System.Security.SecurityException)
        {
            error = exception.Message;
            return false;
        }
    }

    internal static string CreateCommand(string executable) => $"\"{executable}\" {App.WatchCodexArgument}";
}
