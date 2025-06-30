using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView.Setting
{
    public class ContextMenuSettingViewModel : BindableBase
    {
        private MenuTree? _menuTree;
        private ContextMenuSource? _contextMenuSource;


        public ContextMenuSettingViewModel()
        {
            if (CommandTable.Current == null) throw new InvalidOperationException();
        }


        public MenuTree? MenuTree
        {
            get { return _menuTree; }
            set { SetProperty(ref _menuTree, value); }
        }


        public void Initialize(ContextMenuSource contextMenuSetting)
        {
            _contextMenuSource = contextMenuSetting;

            MenuTree = _contextMenuSource.MenuTree;
        }


        public void Reset()
        {
            MenuTree?.Reset();
        }


        public TreeListNode<MenuElement>? Add(TreeListNode<MenuElement> element, TreeListNode<MenuElement> target)
        {
            if (MenuTree is null) return null;
            if (target.Value.MenuElementType != MenuElementType.Group) return null;

            target.Add(element);
            target.IsExpanded = true;
            element.IsSelected = true;
            MenuTree.Validate();

            return element;
        }

        public void RemoveSelf(TreeListNode<MenuElement> target)
        {
            if (MenuTree is null) return;

            var parent = target.Parent;
            if (parent != null)
            {
                var next = target.GetNext(false) ?? target.GetPrev();

                parent.Remove(target);
                MenuTreeTools.Validate(parent);

                if (next != null) next.IsSelected = true;
            }
        }

        public void Move(TreeListNode<MenuElement> src, TreeListNode<MenuElement> dst, int delta)
        {
            if (MenuTree is null) return;

            if (src == dst)
            {
                return;
            }

            // 子に移動
            if (delta == 0)
            {
                var parent = dst;
                if (parent is null)
                {
                    return;
                }
                parent.IsExpanded = true;
                src.MoveTo(parent, -1);
                MenuTree.Validate();
            }

            // 階層をまたいだ移動
            else if (src.Parent != dst.Parent)
            {
                var parent = dst.Parent;
                if (parent is null)
                {
                    return;
                }

                var dstIndex = parent.IndexOf(dst);
                if (dstIndex < 0)
                {
                    return;
                }

                if (delta > 0)
                {
                    dstIndex++;
                }

                src.MoveTo(parent, dstIndex);
                MenuTree.Validate();
            }

            // 同一階層での移動
            else
            {
                var parent = src.Parent;
                if (parent is null)
                {
                    return;
                }

                var srcIndex = parent.IndexOf(src);
                if (srcIndex < 0)
                {
                    return;
                }
                var dstIndex = parent.IndexOf(dst);
                if (dstIndex < 0)
                {
                    return;
                }

                if (srcIndex < dstIndex)
                {
                    if (delta < 0)
                    {
                        dstIndex -= 1;
                    }
                }
                else
                {
                    if (delta > 0)
                    {
                        dstIndex += 1;
                    }
                }

                parent.Move(srcIndex, dstIndex);
                MenuTree.Validate();
            }
        }

        public void Copy(TreeListNode<MenuElement> src, TreeListNode<MenuElement> dst, int delta)
        {
            if (MenuTree is null) return;

            if (src == dst)
            {
                return;
            }

            // 子にコピー
            if (delta == 0)
            {
                var parent = dst;
                if (parent is null)
                {
                    return;
                }
                parent.IsExpanded = true;
                var clone = src.Clone();
                parent.Add(clone);
                MenuTree.Validate();
                clone.IsSelected = true;
            }

            // 前後にコピー
            else
            {
                var parent = dst.Parent;
                if (parent is null)
                {
                    return;
                }

                var dstIndex = parent.IndexOf(dst);
                if (dstIndex < 0)
                {
                    return;
                }

                if (delta > 0)
                {
                    dstIndex++;
                }

                var clone = src.Clone();
                parent.Insert(dstIndex, clone);
                MenuTree.Validate();
                clone.IsSelected = true;
            }
        }

    }
}
