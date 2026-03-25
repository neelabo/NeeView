using NeeView.Properties;
using System;


namespace NeeView
{
    public class LoadFailedFormatDialog : LoadFailedDialog
    {
        public LoadFailedFormatDialog(string title, string arg) : base(title, "")
        {
            Argument = arg;
        }

        public string Argument { get; set; }

        protected override string GetTitleString()
        {
            return TextResources.GetFormatString(Title, Argument);
        }

        protected override string GetMessageString(Exception ex)
        {
            return ex.Message;
        }
    }
}
