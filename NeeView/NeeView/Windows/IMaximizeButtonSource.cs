using System.Windows.Controls;

namespace NeeView.Windows
{
    public interface IMaximizeButtonSource
    {
        /// <summary>
        /// 最大化ボタン取得
        /// </summary>
        /// <returns>nullの場合は最大化ボタンは存在しない</returns>
        Button? GetMaximizeButton();

        /// <summary>
        /// 最大化ボタン背景設定
        /// </summary>
        /// <remarks>
        /// スタイルが機能しない場合の代替手段として用意
        /// </remarks>
        void SetMaximizeButtonBackground(CaptionButtonState state);

        /// <summary>
        /// 最大化ボタンにマウスカーソルが重なったときのイベント
        /// </summary>
        void OnMaximizeButtonMouseEnter();

        /// <summary>
        /// 最大化ボタンからマウスカーソルがなくなったときのイベント
        /// </summary>
        void OnMaximizeButtonMouseLeave();
    }

    public enum CaptionButtonState
    {
        Default,
        MouseOver,
        Pressed,
    }
}
