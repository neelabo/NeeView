using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;


namespace NeeView.Windows.Media
{
    /// <summary>
    /// VisualTreeのユーティリティ
    /// </summary>
    public static class VisualTreeUtility
    {
        #region for ListBox

        /// <summary>
        /// ListBox とその要素から、名前を指定してのコントロールを取得する 
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement? GetListBoxItemElement(ListBox listBox, object item, string name)
        {
            return GetListBoxItemElement(GetListBoxItemFromItem(listBox, item), name);
        }

        /// <summary>
        /// ListBox とその要素から、ListBoxItem を取得する
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ListBoxItem GetListBoxItemFromItem(ListBox listBox, object item)
        {
            return (ListBoxItem)(listBox.ItemContainerGenerator.ContainerFromItem(item));
        }

        /// <summary>
        /// ListBox と index から、ListBoxItem を取得する
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ListBoxItem GetListBoxItemFromIndex(ListBox listBox, int index)
        {
            return (ListBoxItem)(listBox.ItemContainerGenerator.ContainerFromIndex(index));
        }

        /// <summary>
        /// ListBoxItem から、名前を指定してコントロールを取得する 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement? GetListBoxItemElement(ListBoxItem item, string name)
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter? myContentPresenter = FindVisualChild<ContentPresenter>(item);
            if (myContentPresenter == null) return null;

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate? myDataTemplate = myContentPresenter.ContentTemplate;
            if (myDataTemplate == null) throw new InvalidOperationException("DataTemplate not exist.");
            return (FrameworkElement)myDataTemplate.FindName(name, myContentPresenter);
        }

        /// <summary>
        /// ListBoxItem から、型、名前を指定してコントロールを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? GetListBoxItemElement<T>(ListBoxItem item, string? name = null)
            where T : FrameworkElement
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter? myContentPresenter = FindVisualChild<ContentPresenter>(item);
            if (myContentPresenter == null) return null;

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate? myDataTemplate = myContentPresenter.ContentTemplate ?? (myContentPresenter.Content as ContentPresenter)?.ContentTemplate;

            if (myDataTemplate != null)
            {
                return myDataTemplate.FindName(name, myContentPresenter) as T;
            }
            else
            {
                return FindVisualChild<T>(item, name);
            }
        }

        #endregion

        #region for ListView

        /// <summary>
        /// ListView とその要素から、ListViewItem を取得する
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ListViewItem GetListViewItemFromItem(ListView listView, object item)
        {
            return (ListViewItem)(listView.ItemContainerGenerator.ContainerFromItem(item));
        }

        #endregion

        #region for TreeView

        /// <summary>
        /// TreeViewの指定した項目のVisualControlを取得する
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="item">指定項目</param>
        /// <param name="name">コントロール名</param>
        /// <returns></returns>
        public static FrameworkElement? GetTreeViewItemElement(TreeView treeView, object item, string name)
        {
            var container = FindContainer<TreeViewItem>(treeView, item);
            if (container == null) return null;
            return GetTreeViewItemElement(container, name);
        }

        /// <summary>
        /// childItemに対応したTreeViewItemの検索
        /// </summary>
        /// <param name="parent">親ノード。TreeView または TreeViewItem</param>
        /// <param name="childItem">TreeViewItem を取得したい item</param>
        /// <returns></returns>
        public static T? FindContainer<T>(ItemsControl parent, object childItem) where T : DependencyObject
        {
            if (parent.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                // Containerが生成されていない
                return null;
            }
            var container = parent.ItemContainerGenerator.ContainerFromItem(childItem);
            if (container != null)
            {
                // parentの子の中にContainerが見つかった
                return container as T;
            }
            // parentの子を親として再帰検索
            foreach (var item in parent.Items)
            {
                if (parent.ItemContainerGenerator.ContainerFromItem(item) is ItemsControl child && child.Items.Count > 0)
                {
                    var result = FindContainer<T>(child, childItem);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// TreViewItem から名前を指定して VisualChildを取得する
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement? GetTreeViewItemElement(TreeViewItem item, string name)
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter? myContentPresenter = FindVisualChild<ContentPresenter>(item);
            if (myContentPresenter == null) return null;

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            return myDataTemplate.FindName(name, myContentPresenter) as FrameworkElement;
        }

        #endregion

        /// <summary>
        /// 型を指定して指定座標にあるコントロールを取得
        /// </summary>
        /// <typeparam name="T">取得するコントロールの型</typeparam>
        /// <param name="visual">調査対象となるビジュアル</param>
        /// <param name="point">ビジュアル上の座標</param>
        /// <returns>取得されたコントロール。なければ null</returns>
        public static T? HitTest<T>(Visual visual, Point point)
            where T : DependencyObject
        {
            var element = HitTest(visual, point);
            if (element is null)
            {
                return null;
            }

            if (element is not T)
            {
                element = GetParentElement<T>(element);
            }

            return element as T;
        }


        /// <summary>
        /// 非表示オブジェクオを除外したヒットテスト
        /// </summary>
        public static DependencyObject? HitTest(Visual reference, Point point)
        {
            DependencyObject? hit = null;

            VisualTreeHelper.HitTest(reference
                , new HitTestFilterCallback(OnHitTestFilterCallback)
                , new HitTestResultCallback(OnHitTestResultCallback)
                , new PointHitTestParameters(point));

            return hit;

            HitTestResultBehavior OnHitTestResultCallback(HitTestResult result)
            {
                hit = result.VisualHit;
                return HitTestResultBehavior.Stop;
            }

            HitTestFilterBehavior OnHitTestFilterCallback(DependencyObject target)
            {
                // 非表示オブジェクトを除外
                if (target is UIElement element && !element.IsVisible)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }

                return HitTestFilterBehavior.Continue;
            }
        }

        /// <summary>
        /// DependencyObjectとその親から、指定した型のコントロールを取得する.
        /// </summary>
        public static T? FindSourceElement<T>(DependencyObject obj, DependencyObject? terminator = null)
            where T : class
        {
            if (obj is not Visual)
            {
                return null;
            }

            var element = obj;
            while (element != terminator)
            {
                if (element is T item)
                {
                    return item;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }


        /// <summary>
        /// DependencyObject から、型、名前を指定して親コントロールを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T? GetParentElement<T>(DependencyObject obj)
            where T : class
        {
            if (obj is not Visual)
            {
                return null;
            }

            var element = obj;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element is T item)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定した親に属しているか判定する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <param name="includeSelf">検索対象に自身を含めるか</param>
        /// <returns></returns>
        public static bool HasParentElement(DependencyObject obj, DependencyObject target, bool includeSelf = false)
        {
            if (target == null) return false;

            if (includeSelf && target == obj) return true;

            var element = obj;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element == target)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// DependencyObject から、型、名前を指定してコントロールを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? GetChildElement<T>(DependencyObject item, string? name = null)
            where T : FrameworkElement
        {
            if (item == null)
            {
                return null;
            }

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter? myContentPresenter = FindVisualChild<ContentPresenter>(item);
            if (myContentPresenter is null) return null;

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate? myDataTemplate = myContentPresenter.ContentTemplate ?? (myContentPresenter.Content as ContentPresenter)?.ContentTemplate;

            if (myDataTemplate != null)
            {
                return myDataTemplate.FindName(name, myContentPresenter) as T;
            }
            else
            {
                return FindVisualChild<T>(item, name);
            }
        }

        /// <summary>
        /// 指定した型の子要素で最初に見つかったビジュアル要素を返す
        /// </summary>
        /// <remark>http://matatabi-ux.hateblo.jp/entry/2014/02/17/075520</remark>
        /// <typeparam name="T">型</typeparam>
        /// <param name="root">探索対象のビジュアル要素</param>
        /// <returns>見つかった場合はその要素</returns>
        public static T? FindVisualChild<T>(DependencyObject root, string? name = null) where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            if (root is T result && (string.IsNullOrEmpty(name) || name.Equals(result.Name, StringComparison.Ordinal)))
            {
                return result;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                var child = FindVisualChild<T>(VisualTreeHelper.GetChild(root, i), name);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        public static List<T>? FindVisualChildren<T>(DependencyObject root) where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            if (root is T result)
            {
                return new List<T>() { result };
            }

            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root))
                .Select(i => FindVisualChildren<T>(VisualTreeHelper.GetChild(root, i)))
                .WhereNotNull()
                .SelectMany(x => x)
                .ToList();

            return children;
        }

        public static string CollectElementText(DependencyObject root)
        {
            if (root == null)
            {
                return "";
            }

            if (root is Button)
            {
                return "";
            }

            var text = GetElementText(root);
            if (!string.IsNullOrEmpty(text))
            {
                return text + System.Environment.NewLine;
            }

            var s = new StringBuilder();
            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                var childText = CollectElementText(VisualTreeHelper.GetChild(root, i));
                if (!string.IsNullOrEmpty(childText))
                {
                    s.Append(childText);
                }
            }
            return s.ToString();
        }


        private static string GetElementText(DependencyObject element)
        {
            return element switch
            {
                TextBox textBox => textBox.Text,
                TextBlock textBlock => GetStringFromInlineCollection(textBlock.Inlines),
                _ => ""
            };
        }

        private static string GetStringFromInlineCollection(InlineCollection inlineCollection)
        {
            var s = new StringBuilder();
            foreach (Inline inline in inlineCollection)
            {
                if (inline is Run run)
                {
                    s.Append(run.Text);
                }
                else if (inline is Span span)
                {
                    s.Append(GetStringFromInlineCollection(span.Inlines));
                }
            }
            return s.ToString();
        }

        /// <summary>
        /// HitTest and Focus
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool HitTestToFocus(Visual reference, Point point)
        {
            HitTestResult result = VisualTreeHelper.HitTest(reference, point);
            var element = result?.VisualHit as FrameworkElement;

            while (element is not null)
            {
                //Debug.WriteLine($"{element}: Name={element.Name}, Focusable={element.Focusable}");
                if (element.Focusable)
                {
                    return element.Focus();
                }
                // TODO: 親の範囲内に子がいることを前提としている
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }

            return false;
        }
    }

}
