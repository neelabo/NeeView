using CommunityToolkit.Mvvm.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// NowLoading : Model
    /// </summary>
    public class NowLoading : ObservableObject
    {
        static NowLoading() => Current = new NowLoading();
        public static NowLoading Current { get; }

        private bool _isDisplayNowLoading;

        private NowLoading()
        {
            PageFrameBoxPresenter.Current.SubscribeIsLoadingChanged(
                (s, e) => IsDisplayNowLoading = e.IsLoading);
        }

        /// <summary>
        /// IsDisplayNowLoading property.
        /// </summary>
        public bool IsDisplayNowLoading
        {
            get { return _isDisplayNowLoading; }
            set { SetProperty(ref _isDisplayNowLoading, value); }
        }

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
