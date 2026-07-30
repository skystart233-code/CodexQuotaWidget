namespace CodexQuotaWidget.Core;

/// <summary>
/// Separates the Codex Desktop app from a similarly named CLI process.
/// The desktop executable is packaged as Codex.exe and either owns a window
/// or lives in its MSIX package directory.
/// </summary>
public static class CodexDesktopProcessClassifier
{
    public static bool IsCodexDesktopPackage(string? packageFamilyName) =>
        packageFamilyName?.StartsWith("OpenAI.Codex_", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsDesktopApp(string processName, string? executablePath, bool hasMainWindow)
    {
        if (!string.Equals(processName, "Codex", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (hasMainWindow)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        var path = executablePath.Replace('/', '\\');
        return path.Contains("\\WindowsApps\\OpenAI.Codex_", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith("\\app\\Codex.exe", StringComparison.OrdinalIgnoreCase);
    }
}
