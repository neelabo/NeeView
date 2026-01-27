using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// サムネイル用インターフェイス
    /// </summary>
    public interface IThumbnail : INotifyPropertyChanged
    {
        bool IsValid { get; }
        bool IsUniqueImage { get; }
        bool IsNormalImage { get; }
        Brush Background { get; }
        ImageSource? ImageSource { get; }
    }
}
