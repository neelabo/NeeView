namespace NeeView
{
    /// <summary>
    /// Progress 報告データ
    /// </summary>
    public class ProgressContext
    {
        public ProgressContext(string message)
            : this(message, 0.0, false)
        {
        }

        public ProgressContext(string message, double progress)
            : this(message, progress, true)
        {
        }

        public ProgressContext(string message, double progress, bool isProgressVisible)
        {
            Message = message;
            ProgressValue = progress;
            IsProgressVisible = isProgressVisible;
        }


        public string Message { get; set; } = "";
        public double ProgressValue { get; set; }
        public bool IsProgressVisible { get; set; }
    }
}
