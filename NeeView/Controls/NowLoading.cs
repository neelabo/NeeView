using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// NowLoading : Model
    /// </summary>
    public class NowLoading : BindableBase
    {
        static NowLoading() => Current = new NowLoading();
        public static NowLoading Current { get; }

        private NowLoading()
        {
            PageFrameBoxPresenter.Current.Loading +=
                (s, e) => IsDisplayNowLoading = e.Path != null;
        }

        /// <summary>
        /// IsDisplayNowLoading property.
        /// </summary>
        public bool IsDisplayNowLoading
        {
            get { return _IsDisplayNowLoading; }
            set { if (_IsDisplayNowLoading != value) { _IsDisplayNowLoading = value; RaisePropertyChanged(); } }
        }

        private bool _IsDisplayNowLoading;

        public void SetLoading(string message)
        {
            IsDisplayNowLoading = true;
        }

        public void ResetLoading()
        {
            IsDisplayNowLoading = false;
        }
    }

}
