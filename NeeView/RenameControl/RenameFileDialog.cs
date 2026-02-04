using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class RenameFileDialog
    {
        public static Result ShowDialog(string path, string title)
        {
            var directory = System.IO.Path.GetDirectoryName(path) ?? "";
            var filename = System.IO.Path.GetFileName(path);

            var context = new RenameContext(filename);
            context.IsSelectFileNameBody = true;
            context.IsInvalidFileNameChars = true;
            var dialog = new MessageDialog(new RenameDialogComponent(context), title);
            dialog.Commands.AddRange(UICommands.OKCancel);

            var result = dialog.ShowDialog();
            if (result.IsPossible && !string.IsNullOrWhiteSpace(context.NewText))
            {
                return new Result(true, path, System.IO.Path.Combine(directory, context.NewText));
            }
            else
            {
                return new Result(false, path, path);
            }
        }


        private class RenameDialogComponent : BindableBase, IMessageDialogContentComponent
        {
            private readonly TextBox _textBox;
            private readonly RenameContext _context;


            public RenameDialogComponent(RenameContext context)
            {
                _context = context;

                _textBox = new TextBox() { Padding = new Thickness(5.0) };
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _textBox.SetBinding(TextBox.TextProperty, new Binding(nameof(_context.Text)) { Source = _context, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            }


            public event EventHandler? Decide;

            public object Content => _textBox;


            public void OnLoaded(object sender, RoutedEventArgs e)
            {
                // 拡張子以外を選択状態にする
                string name = _context.IsSelectFileNameBody ? LoosePath.GetFileNameWithoutExtension(_context.Text) : _context.Text;
                _textBox.Select(0, name.Length);

                // 表示とともにフォーカスする
                _textBox.Focus();
            }

            private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                {
                    Decide?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                }
            }
        }


        public record class Result(bool IsPossible, string OldPath, string NewPath);
    }
}
