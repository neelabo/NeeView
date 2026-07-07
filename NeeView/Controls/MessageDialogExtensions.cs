using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    public static class MessageDialogExtensions
    {
        /// <summary>
        /// Show MessageDialog
        /// </summary>
        public static MessageDialogResult ShowDialog(this MessageDialogContext context, Window? owner)
        {
            owner ??= MessageDialog.OwnerWindow;

            var window = new Window()
            {
                Style = App.Current.TryFindResource("ChromeDialogStyle") as Style,
                UseLayoutRounding = true,
                Title = "NeeView",
                Width = 600.0,
                SizeToContent = SizeToContent.Height,
                ResizeMode = ResizeMode.NoResize,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = MessageDialog.IsShowInTaskBar || owner is null,
                Content = new MessageDialogControl(new MessageDialogControlViewModel(context))
            };

            window.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    window.Close();
                    e.Handled = true;
                }
            };

            context.ResultCommand = null;
            context.Decided += Context_Decided;
            context.BeginShow();

            var result = window.ShowDialog();

            context.EndShow();
            context.Decided -= Context_Decided;

            var command = (result != null) ? context.ResultCommand : context.GetCancelCommand();

            return new MessageDialogResult(context.ResultCommand);


            void Context_Decided(object? sender, DialogContextDecidedEventArgs e)
            {
                if (e.ResultCommand != null)
                {
                    window.DialogResult = true;
                }

                window.Close();
            }
        }

        /// <summary>
        /// Show PopupDialog
        /// </summary>
        public static MessageDialogResult ShowPopupDialog(this MessageDialogContext context, Window owner)
        {
            var content = new MessageDialogControl(new MessageDialogControlViewModel(context));

            var borderStyle = App.Current.TryFindResource("ModalDialogBorderStyle") as Style;

            var popup = new Popup
            {
                StaysOpen = false,
                PlacementTarget = owner,
                Placement = PlacementMode.Center,
                AllowsTransparency = true,
                Child = new Border()
                {
                    Style = borderStyle,
                    Child = content,
                }
            };

            context.Decided += Context_Decided;
            context.BeginShow();

            DispatcherFrame frame = new DispatcherFrame();

            popup.Closed += (s, e) =>
            {
                frame.Continue = false;
            };

            popup.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    context.Decide(false);
                    e.Handled = true;
                }
            };

            popup.IsOpen = true;

            Dispatcher.PushFrame(frame);

            context.EndShow();
            context.Decided -= Context_Decided;

            return new MessageDialogResult(context.ResultCommand);


            void Context_Decided(object? sender, DialogContextDecidedEventArgs e)
            {
                popup.IsOpen = false;
            }
        }
    }
}
