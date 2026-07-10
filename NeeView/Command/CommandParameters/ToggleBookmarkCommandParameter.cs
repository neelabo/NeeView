using Generator.Equals;
using NeeView.Windows.Property;
using System.Windows.Controls;

namespace NeeView
{
    [Equatable(Explicit = false)]
    public partial class ToggleBookmarkCommandParameter : ToggleCommandParameter
    {
        [DefaultEquality] private string? _folder;

        [PropertyMember(Orientation = Orientation.Vertical)]
        public string? Folder
        {
            get => _folder;
            set => SetProperty(ref _folder, FixFolderPath(value));
        }

        private string? FixFolderPath(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var query = new QueryPath(s, QueryScheme.Bookmark);
            if (query.Scheme != QueryScheme.Bookmark)
            {
                return null;
            }

            return query.FullPath;
        }
    }
}
