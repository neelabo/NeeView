using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// NowLoading : ViewModel
    /// </summary>
    public class NowLoadingViewModel : BindableBase
    {
        private readonly NowLoading _model;


        public NowLoadingViewModel(NowLoading model)
        {
            _model = model;

            _model.SubscribePropertyChanged(nameof(_model.IsDisplayNowLoading),
                (_, _) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsDisplayNowLoading))));
        }


        public bool IsDisplayNowLoading
        {
            get => _model.IsDisplayNowLoading;
            set => _model.IsDisplayNowLoading = value;
        }
    }

}
