namespace NeeView.StringTemplate
{
    public class KeyInfo<TSource>
    {
        public delegate string KeyFormatter(TSource source, string format, string suffix);
        
        public KeyInfo(KeyFormatter formatter) : this(formatter, StringFormatChangedAction.None)
        {
        }

        public KeyInfo(KeyFormatter formatter, StringFormatChangedAction changedAction)
        {
            Formatter = formatter;
            ChangedAction = changedAction;
        }

        public KeyFormatter Formatter { get; set; }
        public StringFormatChangedAction ChangedAction { get; set; }
    }
}

