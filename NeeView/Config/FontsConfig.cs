using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class FontsConfig : BindableBase
    {
        private double _fontScale = 1.25;
        private double _menuFontScale = 1.0;
        private double _folderTreeFontScale = 1.0;
        private double _panelFontScale = 1.25;
        private bool _isClearTypeEnabled = true;

        [JsonPropertyName(nameof(FontName))]
        [JsonInclude]
        public string? _fontName;

        /// <summary>
        /// 標準フォント名
        /// </summary>
        [PropertyMember]
        [JsonIgnore]
        public string FontName
        {
            get { return _fontName ?? SystemVisualParameters.Current.MessageFontName; }
            set { SetProperty(ref _fontName, (string.IsNullOrWhiteSpace(value) || value == SystemVisualParameters.Current.MessageFontName) ? null : value); }
        }

        /// <summary>
        /// 標準フォントスケール
        /// </summary>
        [PropertyPercentFontSize(FontType.Message, 1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double FontScale
        {
            get { return _fontScale <= 0.0 ? 1.25 : _fontScale; }
            set { SetProperty(ref _fontScale, value); }
        }

        /// <summary>
        /// メニューフォントスケール
        /// </summary>
        [PropertyPercentFontSize(FontType.Menu, 1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double MenuFontScale
        {
            get { return _menuFontScale <= 0.0 ? 1.0 : _menuFontScale; }
            set { SetProperty(ref _menuFontScale, value); }
        }

        /// <summary>
        /// フォルダーツリーのフォントスケール
        /// </summary>
        [PropertyPercentFontSize(FontType.Message, 1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double FolderTreeFontScale
        {
            get { return _folderTreeFontScale <= 0.0 ? 1.0 : _folderTreeFontScale; }
            set { SetProperty(ref _folderTreeFontScale, value); }
        }

        /// <summary>
        /// パネルフォントスケール
        /// </summary>
        [PropertyPercentFontSize(FontType.Message, 1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double PanelFontScale
        {
            get { return _panelFontScale <= 0.0 ? 1.25 : _panelFontScale; }
            set { SetProperty(ref _panelFontScale, value); }
        }

        /// <summary>
        /// ClearType
        /// </summary>
        [PropertyMember]
        public bool IsClearTypeEnabled
        {
            get { return _isClearTypeEnabled; }
            set { SetProperty(ref _isClearTypeEnabled, value); }
        }

    }
}
