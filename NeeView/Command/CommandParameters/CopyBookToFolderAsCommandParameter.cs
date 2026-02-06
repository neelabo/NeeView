using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class CopyBookToFolderAsCommandParameter : CommandParameter
    {
        private int _index;

        /// <summary>
        /// 選択されたフォルダーの番号。0 は未選択
        /// </summary>
        [PropertyMember(NoteConverter = typeof(IntToDestinationFolderString))]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, Math.Max(0, value)); }
        }
    }
}
