using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows.Media;

namespace NeeView
{
    public class BackgroundConfig : BindableBase
    {
        private BackgroundType _backgroundType = BackgroundType.Black;
        private BrushSource _customBackground = new();
        private Color _pageBackgroundColor = Colors.Transparent;
        private bool _isPageBackgroundChecker;


        [PropertyMember]
        public BackgroundType BackgroundType
        {
            get { return _backgroundType; }
            set { SetProperty(ref _backgroundType, value); }
        }

        [PropertyMember]
        [PropertyMapLabel("Word.CustomBackground")]
        public BrushSource CustomBackground
        {
            get { return _customBackground; }
            set { SetProperty(ref _customBackground, value ?? new BrushSource()); }
        }

        // ページの背景色。透過画像用
        [PropertyMember]
        public Color PageBackgroundColor
        {
            get { return _pageBackgroundColor; }
            set { SetProperty(ref _pageBackgroundColor, value); }
        }

        // ページ背景は格子模様
        [PropertyMember]
        public bool IsPageBackgroundChecker
        {
            get { return _isPageBackgroundChecker; }
            set { SetProperty(ref _isPageBackgroundChecker, value); }
        }

    }
}
