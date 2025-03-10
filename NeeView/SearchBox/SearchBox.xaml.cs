using NeeView.Threading;
using NeeView.Windows.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// SearchBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public readonly static RoutedCommand DeleteAction = new RoutedCommand("DeleteAction", typeof(SearchBox));

        private readonly DelayAction _delayAction = new();
        private int _requestSearchBoxFocusValue;


        public SearchBox()
        {
            InitializeComponent();

            this.CommandBindings.Add(new CommandBinding(DeleteAction, DeleteAction_Execute));

            this.SearchBoxRoot.DataContext = this;
            this.SearchBoxRoot.IsKeyboardFocusWithinChanged += SearchBoxRoot_IsKeyboardFocusWithinChanged;
        }


        /// <summary>
        /// 検索エラーメッセージ
        /// </summary>
        public string SearchKeywordErrorMessage
        {
            get { return (string)GetValue(SearchKeywordErrorMessageProperty); }
            set { SetValue(SearchKeywordErrorMessageProperty, value); }
        }

        public static readonly DependencyProperty SearchKeywordErrorMessageProperty =
            DependencyProperty.Register("SearchKeywordErrorMessage", typeof(string), typeof(SearchBox), new PropertyMetadata(""));

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 検索キーワード候補。検索履歴とか。
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SearchBox), new PropertyMetadata(null));

        /// <summary>
        /// 検索コマンド
        /// </summary>
        public ICommand? SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(SearchBox), new PropertyMetadata(null));


        /// <summary>
        /// 履歴削除コマンド
        /// </summary>
        public ICommand? DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(SearchBox), new PropertyMetadata(null));


        /// <summary>
        /// キーボードフォーカス変更
        /// </summary>
        private void SearchBoxRoot_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.SearchBoxRoot.IsKeyboardFocusWithin)
            {
                Text = this.SearchBoxComboBox.Text;
                Search();
            }
        }

        /// <summary>
        /// クリアボタン
        /// </summary>
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Text = "";
        }

        /// <summary>
        /// 単キーのショートカット無効
        /// </summary>
        private void Control_KeyDown_IgnoreSingleKeyGesture(object? sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
        }

        /// <summary>
        /// キー入力
        /// </summary>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);

            if (e.Key == Key.Enter)
            {
                Text = this.SearchBoxComboBox.Text;
                Search();
            }
        }

        /// <summary>
        /// テキストボックスのテキスト 遅延反映
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OriginalSource is not TextBox textBox) return;

            _delayAction.Request(() =>
            {
                this.Text = textBox.Text;
            },
            TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        public void FocusAsync()
        {
            if (Interlocked.Exchange(ref _requestSearchBoxFocusValue, 1) == 0)
            {
                _ = FocusSearchBoxAsync(); // 非同期
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカスをあわせる。
        /// </summary>
        /// <returns></returns>
        private async Task FocusSearchBoxAsync()
        {
            // 表示が間に合わない場合があるので繰り返しトライする
            for (int i = 0; i < 10; i++)
            {
                var searchBox = this;
                if (searchBox != null && searchBox.IsLoaded && searchBox.IsVisible && this.IsVisible)
                {
                    searchBox.FocusEditableTextBox();
                    var isFocused = searchBox.IsKeyboardFocusWithin;
                    //Debug.WriteLine($"Focus: {isFocused}");
                    if (isFocused) break;
                }

                //Debug.WriteLine($"Focus: ready...");
                await Task.Delay(100);
            }

            Interlocked.Exchange(ref _requestSearchBoxFocusValue, 0);
            //Debug.WriteLine($"Focus: done.");
        }

        /// <summary>
        /// テキストボックスにフォーカス
        /// </summary>
        private bool FocusEditableTextBox()
        {
            return this.SearchBoxComboBox.Focus();
        }

        /// <summary>
        /// 検索実行
        /// </summary>
        private void Search()
        {
            if (SearchCommand?.CanExecute(null) == true)
            {
                SearchCommand?.Execute(null);
            }
        }

        /// <summary>
        /// 履歴削除実行
        /// </summary>
        private void DeleteAction_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var content = e.Parameter as string;
            if (string.IsNullOrEmpty(content)) return;

            DeleteCommand?.Execute(content);
        }
    }
}
