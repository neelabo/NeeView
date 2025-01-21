using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class Config : BindableBase
    {
        private static Config? _current;
        public static Config Current
        {
            get
            {
                Debug.WriteLine(_current != null, "*** Current is undefined. ***");
                return _current ??= new Config();
            }
        }

        public static void SetCurrent(Config config)
        {
            Debug.Assert(_current is null, "Already set.");
            _current = config;
        }

        public Config()
        {
            View.SetBookSettingSource(BookSetting);
        }

        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public PageViewRecorderConfig PageViewRecorder { get; set; } = new PageViewRecorderConfig();

        [PropertyMapLabel("@Word.Bookmark")]
        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PlaylistConfig Playlist { get; set; } = new PlaylistConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public FontsConfig Fonts { get; set; } = new FontsConfig();

        public BackgroundConfig Background { get; set; } = new BackgroundConfig();

        public WindowTitleConfig WindowTitle { get; set; } = new WindowTitleConfig();

        public PageTitleConfig PageTitle { get; set; } = new PageTitleConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public NoticeConfig Notice { get; set; } = new NoticeConfig();

        public MenuBarConfig MenuBar { get; set; } = new MenuBarConfig();

        public SliderConfig Slider { get; set; } = new SliderConfig();

        public FilmStripConfig FilmStrip { get; set; } = new FilmStripConfig();

        public MainViewConfig MainView { get; set; } = new MainViewConfig();

        public PanelsConfig Panels { get; set; } = new PanelsConfig();

        [PropertyMapLabel("@Word.Bookshelf")]
        public BookshelfConfig Bookshelf { get; set; } = new BookshelfConfig();

        public InformationConfig Information { get; set; } = new InformationConfig();

        public NavigatorConfig Navigator { get; set; } = new NavigatorConfig();

        public PageListConfig PageList { get; set; } = new PageListConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public EffectConfig Effect { get; set; } = new EffectConfig();

        public ControlConfig Control { get; set; } = new ControlConfig();

        public ImageEffectConfig ImageEffect { get; set; } = new ImageEffectConfig();

        public ImageCustomSizeConfig ImageCustomSize { get; set; } = new ImageCustomSizeConfig();

        public ImageTrimConfig ImageTrim { get; set; } = new ImageTrimConfig();

        public ImageDotKeepConfig ImageDotKeep { get; set; } = new ImageDotKeepConfig();

        public ImageGridConfig ImageGrid { get; set; } = new ImageGridConfig();

        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new ImageResizeFilterConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public TouchConfig Touch { get; set; } = new TouchConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public BookConfig Book { get; set; } = new BookConfig();

        public BookSettingConfig BookSetting { get; set; } = new BookSettingConfig();

        public BookSettingConfig BookSettingDefault { get; set; } = new BookSettingConfig();

        public BookSettingPolicyConfig BookSettingPolicy { get; set; } = new BookSettingPolicyConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();


        #region Obsolete

        [Obsolete("no used"), Alternative(nameof(Playlist), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Pagemark
        {
            get { return null; }
            set { }
        }

        #endregion

        #region Validate

        // 誤字修正 (WindowTittle -> WindowTitle)
        [Obsolete("Typo"), Alternative(nameof(WindowTitle), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public WindowTitleConfig? WindowTittle
        {
            get { return this.WindowTitle; }
            set { if (value is not null) this.WindowTitle = value; }
        }

        #endregion

    }
}

