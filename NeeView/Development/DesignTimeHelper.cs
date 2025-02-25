using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public class DesignTimeHelper
    {
        public static bool IsInDesignMode()
        {
#if DEBUG
            return DesignerProperties.GetIsInDesignMode(new DependencyObject());
#else
            return false;
#endif
        }
    }
}
