using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// RenameManager.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class RenameManager : Canvas, INotifyPropertyChanged
    {
        public RenameManager()
        {
            Loaded += RenameManager_Loaded;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsRenaming =>
            this.Children != null && this.Children.Count > 0;


        public UIElement? RenameElement
        {
            get
            {
                if (this.Children != null && this.Children.Count > 0)
                {
                    var renameControl = (RenameControl)this.Children[0];
                    return renameControl.Target ?? renameControl.StoredFocusTarget;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// イベント初期化
        /// </summary>
        private void RenameManager_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Deactivated += (s, e) => CloseAll();
            }
        }

        /// <summary>
        /// 登録
        /// </summary>
        public void Add(RenameControl rename)
        {
            if (rename is null) throw new ArgumentException("element must be RenameControl");

            if (this.Children.Contains(rename)) return;

            rename.SyncLayout();

            this.Children.Add(rename);

            rename.SetTargetVisibility(Visibility.Hidden);

            RaisePropertyChanged(nameof(IsRenaming));
        }


        /// <summary>
        /// 登録解除
        /// </summary>
        public void Remove(RenameControl rename)
        {
            if (rename is null) throw new ArgumentException("element must be RenameControl");

            rename.SetTargetVisibility(Visibility.Visible);

            // NOTE: ウィンドウのディアクティブタイミングで閉じたときに再度アクティブ化するのを防ぐためにタイミングをずらす。動作原理不明。
            AppDispatcher.BeginInvoke(() =>
            {
                this.Children.Remove(rename);
                RaisePropertyChanged(nameof(IsRenaming));
            });
        }

        /// <summary>
        /// すべて閉じる
        /// </summary>
        public void CloseAll(bool isSuccess = true, bool isRestoreFocus = true)
        {
            if (this.Children != null && this.Children.Count > 0)
            {
                var renames = this.Children.OfType<RenameControl>().ToList();
                foreach (var rename in renames)
                {
                    // NOTE: 非同期で実行
                    _ = rename.CloseAsync(isSuccess, isRestoreFocus);
                }
            }
        }

        /// <summary>
        /// renameコントロールをターゲットの位置に合わせる
        /// </summary>
        public void SyncLayout()
        {
            if (this.Children != null && this.Children.Count > 0)
            {
                var renames = this.Children.OfType<RenameControl>().ToList();
                foreach (var rename in renames)
                {
                    rename.SyncLayout();
                }
            }
        }


        /// <summary>
        /// elementの所属するRenameManagerを取得
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static RenameManager? GetRenameManager(UIElement element)
        {
            RenameManager? renameManager = null;

            var window = Window.GetWindow(element);
            if (window is IHasRenameManager hasRenameManager)
            {
                renameManager = hasRenameManager.GetRenameManager();
                Debug.Assert(renameManager != null);
                return renameManager;
            }

            return null;
        }
    }

}
