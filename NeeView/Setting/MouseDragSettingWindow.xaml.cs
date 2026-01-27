using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NeeView.Setting
{

    /// <summary>
    /// MouseDragSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseDragSettingWindow : Window
    {
        private readonly DragActionCollection _memento;
        private readonly string _key;

        public MouseDragSettingWindow(string key, MouseDragSettingWindowTab start)
        {
            InitializeComponent();

            this.Loaded += MouseDragSettingWindow_Loaded;
            this.KeyDown += MouseDragSettingWindow_KeyDown;

            _memento = DragActionTable.Current.CreateDragActionCollection(false);
            _key = key;

            var note = DragActionTable.Current.Elements[_key].Note;
            this.Title = $"{note} - {TextResources.GetString("MouseDragSettingWindow.Title")}";

            this.MouseGesture.Initialize(_memento, key);
            this.Parameter.Initialize(_memento, key);

            switch (start)
            {
                case MouseDragSettingWindowTab.MouseGesture:
                    this.MouseGestureTab.IsSelected = true;
                    break;
                case MouseDragSettingWindowTab.Parameter:
                    this.ParameterTab.IsSelected = true;
                    break;
            }

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }


        private void MouseDragSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.OkButton.Focus();
        }

        private void MouseDragSettingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.MouseGesture.Decide();
            DragActionTable.Current.RestoreDragActionCollection(_memento);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }



    public enum MouseDragSettingWindowTab
    {
        MouseGesture,
        Parameter,
    }



    public class DragToken
    {
        public DragToken()
        {
        }

        // ジェスチャー（１ジェスチャー）
        public DragKey Gesture { get; set; } = DragKey.Empty;

        // 競合しているコマンド群
        public List<string>? Conflicts { get; set; }

        // 競合メッセージ
        public string? OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }
}
