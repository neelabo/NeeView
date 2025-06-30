using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows
{
    /// <summary>
    ///  FontIcon based on Segoe MDL2 assets
    /// </summary>
    public class FontIcon : TextBlock
    {
        internal static readonly FontFamily SegoeMLD2Assets;
        static FontIcon()
        {
            SegoeMLD2Assets = new FontFamily("Segoe MDL2 Assets");
        }

        public FontIcon()
        {
            FontFamily = SegoeMLD2Assets;
        }

        public FontIcon(string text) : this()
        {
            Text = text;
        }
    }
}
