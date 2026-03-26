using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class ToggleBookmarkCommandParameter : CommandParameter
    {
        [DefaultEquality] private string? _folder;

        [PropertyMember]
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
