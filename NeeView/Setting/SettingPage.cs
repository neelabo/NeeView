using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定ウィンドウのページ
    /// </summary>
    public class SettingPage : BindableBase
    {
        private UIElement? _content;
        private bool _isSelected;


        public SettingPage(string header)
        {
            this.Header = header;
        }

        public SettingPage(string header, List<SettingItem> items)
            : this(header)
        {
            this.Items = items;
        }

        public SettingPage(string header, List<SettingItem> items, params SettingPage[] children)
            : this(header, items)
        {
            this.Children = children.Where(e => e != null).ToList();
        }


        /// <summary>
        /// ページ名
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// 子ページ
        /// </summary>
        public List<SettingPage>? Children { get; protected set; }

        /// <summary>
        /// 項目
        /// </summary>
        public List<SettingItem>? Items { get; protected set; }

        /// <summary>
        /// TreeViewで、このノードが選択されているか
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; RaisePropertyChanged(); } }
        }

        // 最初から開いた状態にする
        public bool IsExpanded { get; set; } = true;

        /// <summary>
        /// 表示コンテンツ
        /// </summary>
        public UIElement? Content
        {
            get { return _content ?? (_content = CreateContent()); }
        }

        /// <summary>
        /// スクロールビュー？
        /// </summary>
        public bool IsScrollEnabled { get; set; } = true;

        /// <summary>
        /// リセットボタン
        /// </summary>
        public bool IsResetButtonEnabled { get; set; } = true;
        public bool IsResetButtonConfirm { get; set; } = true;
        public Thickness ResetButtonMargin { get; set; } = new Thickness(0, 10, 10, 15);

        /// <summary>
        /// 表示ページ。
        /// コンテンツがない場合、子のページを返す
        /// </summary>
        public SettingPage? DisplayPage
        {
            get { return (this.Items != null) ? this : this.Children?.FirstOrDefault(); }
        }

        public void ClearContentCache()
        {
            _content = null;
        }

        private UIElement? CreateContent()
        {
            if (this.Items == null)
            {
                return null;
            }

            var dockPanel = new DockPanel();
            dockPanel.MinWidth = 256;
            dockPanel.SetResourceReference(RenderOptions.ClearTypeHintProperty, "Window.ClearTypeHint");

            foreach (var item in this.Items)
            {
                var itemContent = item.CreateContent();
                if (itemContent != null)
                {
                    DockPanel.SetDock(itemContent, Dock.Top);
                    dockPanel.Children.Add(itemContent);
                }
            }

            dockPanel.Margin = new Thickness(20, 0, 0, 0);
            UIElement panel = dockPanel;

            if (this.IsScrollEnabled)
            {
                dockPanel.Margin = new Thickness(20, 0, 20, 20);
                dockPanel.LastChildFill = false;

                var scrollViewer = new ScrollViewer();
                scrollViewer.PanningMode = PanningMode.VerticalOnly;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollViewer.Content = dockPanel;
                scrollViewer.Focusable = false;
                panel = scrollViewer;
            }

            if (IsResetButtonEnabled)
            {
                var button = new Button();
                button.Content = TextResources.GetString("Word.Reset");
                button.HorizontalAlignment = HorizontalAlignment.Right;
                button.Margin = ResetButtonMargin;
                button.Padding = new Thickness(10, 5, 10, 5);
                button.MinWidth = 100.0;
                button.TabIndex = 2;
                button.Click += ResetButton_Click;
                DockPanel.SetDock(button, Dock.Bottom);

                var topPanel = new DockPanel();
                topPanel.Children.Add(button);
                topPanel.Children.Add(panel);

                panel = topPanel;
            }

            return panel;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsResetButtonConfirm)
            {
                var dialog = new MessageDialog(TextResources.GetString("SettingResetDialog.Message"), TextResources.GetString("SettingResetDialog.Title"));
                dialog.Commands.Add(UICommands.OK);
                dialog.Commands.Add(UICommands.Cancel);
                var result = dialog.ShowDialog(Window.GetWindow(_content));
                if (result.IsPossible)
                {
                    InitializeValue();
                }
            }
            else
            {
                InitializeValue();
            }
        }

        public void SetItems(List<SettingItem> items)
        {
            Items = items;
            _content = null;
            RaisePropertyChanged(nameof(Content));
        }

        public IEnumerable<SettingItem> GetItemCollection()
        {
            if (Items == null) yield break;

            foreach (var item in Items)
            {
                foreach (var subItem in item.GetItemCollection())
                {
                    yield return subItem;
                }
            }
        }

        public string GetSearchText()
        {
            return Header;
        }

        public virtual void InitializeValue()
        {
            if (Items == null) return;

            foreach (var item in Items)
            {
                item.InitializeValue();
            }
        }
    }
}
