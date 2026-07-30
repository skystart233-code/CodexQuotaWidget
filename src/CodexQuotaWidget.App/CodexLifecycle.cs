using System.Diagnostics;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.App;

internal static class CodexLifecycle
{
    public static bool IsDesktopRunning()
    {
        foreach (var process in Process.GetProcessesByName("Codex"))
        {
            try
            {
                string? executablePath = null;
                var hasMainWindow = false;
                try
                {
                    executablePath = process.MainModule?.FileName;
                }
                catch (InvalidOperationException)
                {
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                try
                {
                    hasMainWindow = process.MainWindowHandle != IntPtr.Zero;
                }
                catch (InvalidOperationException)
                {
                }

                var packageFamilyName = WindowsPackageIdentity.TryGetFamilyName(process.Handle);
                if (CodexDesktopProcessClassifier.IsCodexDesktopPackage(packageFamilyName) ||
                    CodexDesktopProcessClassifier.IsDesktopApp(process.ProcessName, executablePath, hasMainWindow))
                {
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        return false;
    }

    public static bool TryLaunchManagedWidget(out string? error)
        => TryLaunch(App.ManagedByCodexArgument, out error);

    public static bool TryLaunchWatcher(out string? error)
        => TryLaunch(App.WatchCodexArgument, out error);

    private static bool TryLaunch(string argument, out string? error)
    {
        try
        {
            var executable = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executable))
            {
                error = "The widget executable path could not be determined.";
                return false;
            }

            Process.Start(new ProcessStartInfo(executable, argument)
            {
                UseShellExecute = true
            });
            error = null;
            return true;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            error = exception.Message;
            return false;
        }
    }
}
