using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// RenameControl用ソース
    /// </summary>
    public class RenameControlSource
    {
        public RenameControlSource(UIElement target, TextBlock? textBlock, string text)
        {
            TargetContainer = target;
            Target = textBlock;
            Text = text;
        }

        public RenameControlSource(UIElement target, TextBlock textBlock) : this(target, textBlock, textBlock.Text)
        {
        }

        /// <summary>
        /// フォーカスを戻すコントロール
        /// </summary>
        public UIElement TargetContainer { get; }

        /// <summary>
        /// 名前変更を行うTextBlock
        /// </summary>
        public TextBlock? Target { get; }

        /// <summary>
        /// 変更前テキスト
        /// </summary>
        public string Text { get; }
    }
}
