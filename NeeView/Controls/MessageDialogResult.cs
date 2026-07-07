namespace NeeView
{
    /// <summary>
    /// MessageDialog Result
    /// </summary>
    public class MessageDialogResult
    {
        public MessageDialogResult(UICommand? command)
        {
            Command = command;
        }

        public UICommand? Command { get; }
        public bool IsPossible => Command != null && Command.IsPossible;
    }
}
