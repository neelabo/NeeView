using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    public static unsafe class IShellItemExtensions
    {
        internal static string? GetPath(this IShellItem item)
        {
            if (item == null) return null;

            PWSTR pName = default;

            item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &pName);

            try
            {
                if (pName.Value is null) return null;

                return pName.ToString();
            }
            finally
            {
                PInvoke.CoTaskMemFree(pName);
            }
        }
    }
}

