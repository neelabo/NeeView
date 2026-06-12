using Generator.Equals;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class CopyBookToFolderAsCommandParameter : CommandParameter
    {
        [DefaultEquality] private int _index;

        /// <summary>
        /// 選択されたフォルダーの番号。0 は未選択
        /// </summary>
        [PropertyIntegers(MapGenerator = typeof(DestinationFolderMapGenerator))]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, Math.Max(0, value)); }
        }
    }
}
