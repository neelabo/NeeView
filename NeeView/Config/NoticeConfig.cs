using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class NoticeConfig : BindableBase
    {
        [DefaultEquality] private ShowMessageStyle _noticeShowMessageStyle = ShowMessageStyle.Normal;
        [DefaultEquality] private ShowMessageStyle _bookNameShowMessageStyle = ShowMessageStyle.Normal;
        [DefaultEquality] private ShowMessageStyle _commandShowMessageStyle = ShowMessageStyle.Normal;
        [DefaultEquality] private ShowMessageStyle _gestureShowMessageStyle = ShowMessageStyle.Normal;
        [DefaultEquality] private ShowMessageStyle _nowLoadingShowMessageStyle = ShowMessageStyle.Normal;
        [DefaultEquality] private ShowMessageStyle _viewTransformShowMessageStyle = ShowMessageStyle.None;
        [DefaultEquality] private bool _isOriginalScaleShowMessage;
        [DefaultEquality] private bool _isBusyMarkEnabled = true;
        [DefaultEquality] private bool _isEmptyMessageEnabled = true;


        [PropertyMember]
        public ShowMessageStyle NoticeShowMessageStyle
        {
            get { return _noticeShowMessageStyle; }
            set { SetProperty(ref _noticeShowMessageStyle, value); }
        }

        [PropertyMember]
        public ShowMessageStyle BookNameShowMessageStyle
        {
            get { return _bookNameShowMessageStyle; }
            set { SetProperty(ref _bookNameShowMessageStyle, value); }
        }

        [PropertyMember]
        public ShowMessageStyle CommandShowMessageStyle
        {
            get { return _commandShowMessageStyle; }
            set { SetProperty(ref _commandShowMessageStyle, value); }
        }

        [PropertyMember]
        public ShowMessageStyle GestureShowMessageStyle
        {
            get { return _gestureShowMessageStyle; }
            set { SetProperty(ref _gestureShowMessageStyle, value); }
        }

        [PropertyMember]
        public ShowMessageStyle NowLoadingShowMessageStyle
        {
            get { return _nowLoadingShowMessageStyle; }
            set { SetProperty(ref _nowLoadingShowMessageStyle, value); }
        }

        [PropertyMember]
        public ShowMessageStyle ViewTransformShowMessageStyle
        {
            get { return _viewTransformShowMessageStyle; }
            set { SetProperty(ref _viewTransformShowMessageStyle, value); }
        }

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        [PropertyMember]
        public bool IsOriginalScaleShowMessage
        {
            get { return _isOriginalScaleShowMessage; }
            set { SetProperty(ref _isOriginalScaleShowMessage, value); }
        }

        /// <summary>
        /// 非同期のページ読込処理中のマークを表示する
        /// </summary>
        [PropertyMember]
        public bool IsBusyMarkEnabled
        {
            get { return _isBusyMarkEnabled; }
            set { SetProperty(ref _isBusyMarkEnabled, value); }
        }

        /// <summary>
        /// ページがないメッセージの表示
        /// </summary>
        [PropertyMember]
        public bool IsEmptyMessageEnabled
        {
            get { return _isEmptyMessageEnabled; }
            set { SetProperty(ref _isEmptyMessageEnabled, value); }
        }
    }
}


