namespace NeeView
{
    public class BootSetting
    {
        public string Language { get; set; } = "en";
        public bool IsSplashScreenEnabled { get; set; } = true;
        public bool IsMultiBootEnabled { get; set; }

        internal static BootSetting Create(Config config)
        {
            return new BootSetting()
            {
                Language = config.System.Language,
                IsSplashScreenEnabled = config.StartUp.IsSplashScreenEnabled,
                IsMultiBootEnabled = config.StartUp.IsMultiBootEnabled,
            };
        }
    }

}
