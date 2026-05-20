using System.Windows.Media;

namespace NeeView
{
    public interface ITagItem
    {
        public string Name { get; }
        public Brush? Background { get; }
    }
}
