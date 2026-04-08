using System;


namespace NeeView.StringTemplate
{
    [Flags]
    public enum StringFormatChangedAction
    {
        None = 0,

        FormatChanged = (1 << 0),
        
        ViewContentChanged = (1 << 1),
        
        ScaleChanged = (1 << 2),
        
        StretchChanged = (1 << 3),
    }
}
