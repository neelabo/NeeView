using NeeView.Properties;
using System;


namespace NeeView
{
    /// <summary>
    /// データロードエラーダイアログ
    /// </summary>
    public class LoadFailedDialog
    {
        public LoadFailedDialog(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; set; }
        public string Message { get; set; }
        public UICommand OKCommand { get; set; } = UICommands.OK;
        public UICommand? CancelCommand { get; set; }


        public bool ShowDialog(Exception ex)
        {
            var textBox = new System.Windows.Controls.TextBox()
            {
                IsReadOnly = true,
                Text = TextResources.GetString(Message) + System.Environment.NewLine + ex.Message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
            };

            var dialog = new MessageDialog(textBox, TextResources.GetString(Title));
            dialog.SizeToContent = System.Windows.SizeToContent.Manual;
            dialog.Height = 320.0;
            dialog.ResizeMode = System.Windows.ResizeMode.CanResize;
            dialog.Commands.Add(OKCommand);
            if (CancelCommand != null)
            {
                dialog.Commands.Add(CancelCommand);
            }

            var result = dialog.ShowDialog();
            return result.Command == OKCommand || CancelCommand == null;
        }
    }
}
