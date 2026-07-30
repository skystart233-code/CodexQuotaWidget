using System.Runtime.InteropServices;
using System.Text;

namespace CodexQuotaWidget.App;

internal static class WindowsPackageIdentity
{
    private const int ErrorInsufficientBuffer = 122;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetPackageFamilyName(
        IntPtr process,
        ref uint packageFamilyNameLength,
        StringBuilder? packageFamilyName);

    public static string? TryGetFamilyName(IntPtr processHandle)
    {
        try
        {
            uint length = 0;
            if (GetPackageFamilyName(processHandle, ref length, null) != ErrorInsufficientBuffer || length == 0)
            {
                return null;
            }

            var value = new StringBuilder(checked((int)length));
            return GetPackageFamilyName(processHandle, ref length, value) == 0 ? value.ToString() : null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }
}
