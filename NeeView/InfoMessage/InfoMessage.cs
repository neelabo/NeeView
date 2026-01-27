namespace NeeView
{
    public enum InfoMessageType
    {
        Notify,
        BookName,
        Command,
        Gesture,
        Loading,
        ViewTransform,
    }

    /// <summary>
    /// 通知表示管理
    /// </summary>
    public class InfoMessage
    {
        static InfoMessage() => Current = new InfoMessage();
        public static InfoMessage Current { get; }

        private InfoMessage()
        {
        }

        private static ShowMessageStyle GetShowMessageStyle(InfoMessageType type)
        {
            return type switch
            {
                InfoMessageType.BookName => Config.Current.Notice.BookNameShowMessageStyle,
                InfoMessageType.Command => Config.Current.Notice.CommandShowMessageStyle,
                InfoMessageType.Gesture => Config.Current.Notice.GestureShowMessageStyle,
                InfoMessageType.Loading => Config.Current.Notice.NowLoadingShowMessageStyle,
                InfoMessageType.ViewTransform => Config.Current.Notice.ViewTransformShowMessageStyle,
                _ => Config.Current.Notice.NoticeShowMessageStyle,
            };
        }

        public NormalInfoMessage NormalInfoMessage { get; } = new NormalInfoMessage();

        public TinyInfoMessage TinyInfoMessage { get; } = new TinyInfoMessage();

        private void SetMessage(ShowMessageStyle style, string message, string? tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            switch (style)
            {
                case ShowMessageStyle.Normal:
                    this.NormalInfoMessage.SetMessage(message, dispTime, bookmarkType);
                    break;
                case ShowMessageStyle.Tiny:
                    this.TinyInfoMessage.SetMessage(tinyMessage ?? message);
                    break;
            }
        }

        public void SetMessage(InfoMessageType type, string message, string? tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            SetMessage(GetShowMessageStyle(type), message, tinyMessage, dispTime, bookmarkType);
        }

        public void ClearMessage(ShowMessageStyle style)
        {
            SetMessage(style, "");
        }

    }
}
