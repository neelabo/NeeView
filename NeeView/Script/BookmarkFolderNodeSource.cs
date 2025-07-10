using System.Collections.Generic;

namespace NeeView
{
    public record class BookmarkFolderNodeSource : ISetParameter
    {
        private readonly BookmarkFolderNode _node;

        public BookmarkFolderNodeSource(BookmarkFolderNode node)
        {
            _node = node;
        }

        [WordNodeMember(AltName = "Word.Name")]
        public string Name
        {
            get { return _node.DisplayName; }
            set { AppDispatcher.Invoke(() => _node.Rename(value)); }
        }

        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
        }


        public void SetParameter(IDictionary<string, object?>? obj)
        {
            if (obj == null) return;
            var name = JavaScriptObjectTools.GetValue<string>(obj, nameof(Name));
            if (name is not null)
            {
                Name = name;
            }
        }
    }

}
