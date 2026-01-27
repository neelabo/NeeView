// TODO: 関数が大きすぎる？細分化を検討

namespace NeeView
{
    /// <summary>
    /// N字スクロールパラメータ
    /// </summary>
    public interface IScrollNTypeParameter
    {
        /// <summary>
        /// スクロールの種類
        /// </summary>
        NScrollType ScrollType { get; set; }

        /// <summary>
        /// スクロール移動量の割合
        /// </summary>
        double Scroll { get; set; }

        /// <summary>
        /// 改行遅延時間
        /// </summary>
        double LineBreakStopTime { get; set; }

        /// <summary>
        /// パノラマモードでは全てのページを１ページとみなす
        /// </summary>
        bool PagesAsOne { get; set; }
    }

}
