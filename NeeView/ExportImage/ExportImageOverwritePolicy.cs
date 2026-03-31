using NeeView.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public interface IExportOverwritePolicy
    {
        string Resolve(string name, IExportOverwriteResolver resolver, ExportImageService? service);
    }


    /// <summary>
    /// 衝突したら失敗とする
    /// </summary>
    public class InvalidExportOverwritePolicy : IExportOverwritePolicy
    {
        public string Resolve(string name, IExportOverwriteResolver resolver, ExportImageService? service)
        {
            if (resolver.Exists(name))
            {
                throw new IOException($"The file already exists: {name}");
            }
            return name;
        }
    }


    /// <summary>
    /// 衝突したら確認ダイアログを表示する
    /// </summary>
    public class ConfirmExportOverwritePolicy : IExportOverwritePolicy
    {
        public string Resolve(string name, IExportOverwriteResolver resolver, ExportImageService? service)
        {
            if (!resolver.Exists(name))
            {
                return name;
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock() { Text = TextResources.GetFormatString("ConfirmFileReplaceDialog.Message", name), Margin = new Thickness(0, 10, 0, 10) });
            stackPanel.Children.Add(resolver.CreateOverwriteContent(name, service));
            var dialog = new MessageDialog(stackPanel, TextResources.GetString("ConfirmFileReplaceDialog.Title"));
            var commandReplace = new UICommand("ConfirmFileReplaceDialog.Replace") { IsPossible = true };
            var commandAddNumber = new UICommand("ConfirmFileReplaceDialog.AddNumber") { IsPossible = true };
            dialog.Commands.Add(commandReplace);
            dialog.Commands.Add(commandAddNumber);
            dialog.Commands.Add(UICommands.Cancel);
            dialog.Width = 800;

            var answer = dialog.ShowDialog();

            if (answer.Command == commandReplace)
            {
                return name;
            }
            else if (answer.Command == commandAddNumber)
            {
                return resolver.CreateUniqueName(name);
            }
            else
            {
                throw new OperationCanceledException();
            }
        }
    }

    /// <summary>
    /// 衝突したらユニーク名にする
    /// </summary>
    public class AddNumberExportOverwritePolicy : IExportOverwritePolicy
    {
        public string Resolve(string name, IExportOverwriteResolver resolver, ExportImageService? service)
        {
            return resolver.CreateUniqueName(name);
        }
    }
}