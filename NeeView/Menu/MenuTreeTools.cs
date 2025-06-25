using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NeeView
{
    public static class MenuTreeTools
    {
        public static ContextMenu? CreateContextMenu(TreeListNode<MenuElement> node)
        {
            if (node.Children == null) return null;
            var contextMenu = new ContextMenu();

            foreach (var element in node.Children)
            {
                var control = CreateMenuControl(element, false);
                if (control != null) contextMenu.Items.Add(control);
            }

            return contextMenu.Items.Count > 0 ? contextMenu : null;
        }

        public static List<object> CreateContextMenuItems(TreeListNode<MenuElement> node)
        {
            if (node.Children == null) return new List<object>();

            var children = node.Children
                .Select(e => CreateMenuControl(e, false))
                .WhereNotNull()
                .ToList();

            return children;
        }

        public static Menu? CreateMenu(TreeListNode<MenuElement> node, bool isDefault)
        {
            var menu = new Menu();
            foreach (var element in CreateMenuItems(node, isDefault))
            {
                menu.Items.Add(element);
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        public static List<object> CreateMenuItems(TreeListNode<MenuElement> node, bool isDefault)
        {
            var collection = new List<object>();

            if (node.Children != null)
            {
                foreach (var element in node.Children)
                {
                    var control = CreateMenuControl(element, isDefault);
                    if (control != null) collection.Add(control);
                }
            }

            return collection;
        }

        public static object? CreateMenuControl(TreeListNode<MenuElement> node, bool isDefault)
        {
            switch (node.Value.MenuElementType)
            {
                case MenuElementType.Command:
                    if (node.Value.CommandName is not null && CommandTable.Current.TryGetValue(node.Value.CommandName, out var command))
                    {
                        return CreateCommandMenuControl(node, command, isDefault);
                    }
                    else
                    {
                        Debug.WriteLine($"Command {node.Value.CommandName} is not defined.");
                        return CreateInvalidMenuControl(node);
                    }

                case MenuElementType.Separator:
                    return new Separator();

                case MenuElementType.Group:
                    {
                        if (node.Children is null) throw new InvalidOperationException();
                        var item = new MenuItem();
                        item.Header = node.Value.Label;
                        foreach (var child in node.Children)
                        {
                            var control = CreateMenuControl(child, isDefault);
                            if (control != null) item.Items.Add(control);
                        }
                        item.IsEnabled = item.Items.Count > 0;
                        return item;
                    }

                case MenuElementType.History:
                    return CreateCommandMenuControl(node, CommandTable.Current.GetElement<LoadRecentBookCommand>(), isDefault);

                case MenuElementType.None:
                    return null;

                default:
                    throw new NotImplementedException();
            }
        }

        private static MenuItem CreateCommandMenuControl(TreeListNode<MenuElement> node, CommandElement command, bool isDefault)
        {
            var item = new MenuItem();

            var menuItem = command.CreateMenuItem(isDefault);
            if (menuItem is not null)
            {
                item = menuItem;
            }
            else
            {
                item.Command = RoutedCommandTable.Current.Commands[node.Value.CommandName ?? "None"];
                item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            }

            item.Header = node.Value.Label;
            item.Tag = node.Value.CommandName;
            var binding = command.CreateIsCheckedBinding();
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            //  右クリックでコマンドパラメーターの設定ウィンドウを開く
            item.MouseRightButtonUp += (s, e) =>
            {
                if (s is MenuItem menuItem && menuItem.Tag is string command)
                {
                    e.Handled = true;
                    MainWindowModel.Current.OpenCommandParameterDialog(command);
                }
            };

            return item;
        }

        private static MenuItem CreateInvalidMenuControl(TreeListNode<MenuElement> node)
        {
            var item = new MenuItem();
            item.Header = node.Value.Label;
            item.IsEnabled = false;
            return item;
        }

        public static void Validate(TreeListNode<MenuElement> node)
        {
            if (node.Value.MenuElementType == MenuElementType.Group)
            {
                var removes = new List<TreeListNode<MenuElement>>();
                var isRemoveNone = node.Children.Count(e => e.Value.MenuElementType != MenuElementType.None) > 0;

                foreach (var child in node.Children)
                {
                    if (child.Value.MenuElementType == MenuElementType.None && isRemoveNone)
                    {
                        Debug.WriteLine($"MenuTree.Validate: Remove EmptyNode");
                        removes.Add(child);
                    }
                    else if (child.Value.MenuElementType == MenuElementType.Command && !IsCommandAllowed(child.Value.CommandName))
                    {
                        Debug.WriteLine($"MenuTree.Validate: Remove CommandNode=\"{child.Value.CommandName}\"");
                        removes.Add(child);
                    }
                    else
                    {
                        Validate(child);
                    }
                }
                foreach (var e in removes)
                {
                    e.RemoveSelf();
                }
                if (node.Children.Count == 0)
                {
                    node.Add(new TreeListNode<MenuElement>(new NoneMenuElement()));
                }
            }
        }

        /// <summary>
        /// コマンドの存在許可。
        /// クローンコマンドやスクリプトコマンド等は実態がなくても自動削除されない。
        /// </summary>
        /// <param name="commandName">コマンドID</param>
        /// <returns>存在許可するなら true</returns>
        private static bool IsCommandAllowed(string? commandName)
        {
            if (commandName is null)
            {
                return false;
            }

            if (CommandTable.Current.ContainsKey(commandName))
            {
                return true;
            }

            if (ScriptCommand.IsScriptCommandName(commandName))
            {
                return true;
            }

            var cloneName = CommandNameSource.Parse(commandName);
            if (cloneName.IsClone)
            {
                if (CommandTable.Current.ContainsKey(cloneName.Name))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEqual(TreeListNode<MenuElement> x, TreeListNode<MenuElement> y)
        {
            if (x.Value.MenuElementType != y.Value.MenuElementType) return false;
            if (x.Value.Label != y.Value.Label) return false;
            if (x.Value.CommandName != y.Value.CommandName) return false;
            if (x.Children != null && y.Children != null)
            {
                if (x.Children.Count != y.Children.Count) return false;
                for (int i = 0; i < x.Children.Count; ++i)
                {
                    if (!IsEqual(x.Children[i], y.Children[i])) return false;
                }
            }
            else if (x.Children != null || y.Children != null) return false;

            return true;
        }

        public static void RaisePropertyChangedAll(TreeListNode<MenuElement> node)
        {
            node.Value.RaisePropertyChangedAll();

            if (node.Value.MenuElementType == MenuElementType.Group && node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    RaisePropertyChangedAll(child);
                }
            }
        }

        // for MenuNode
        public static MenuNode CreateMenuNode(TreeListNode<MenuElement> node)
        {
            var menuNode = new MenuNode(node.Value.Name, node.Value.MenuElementType, node.Value.CommandName);

            if (node.Children.Count > 0)
            {
                menuNode.Children = new List<MenuNode>();
                foreach (var child in node.Children)
                {
                    menuNode.Children.Add(CreateMenuNode(child));
                }
            }

            return menuNode;
        }

        public static TreeListNode<MenuElement> CreateMenuTreeNode(MenuNode menuNode)
        {
            var element = CreateMenuElement(menuNode);

            var node = new TreeListNode<MenuElement>(element);
            if (menuNode.Children != null)
            {
                foreach (var child in menuNode.Children)
                {
                    node.Add(CreateMenuTreeNode(child));
                }
            }

            return node;
        }

        private static MenuElement CreateMenuElement(MenuNode menuNode)
        {
            return menuNode.MenuElementType switch
            {
                MenuElementType.None => new NoneMenuElement(),
                MenuElementType.Group => new GroupMenuElement() { Name = menuNode.Name },
                MenuElementType.Command => new CommandMenuElement() { Name = menuNode.Name, CommandName = menuNode.CommandName },
                MenuElementType.History => new CommandMenuElement() { Name = menuNode.Name, CommandName = CommandElementTools.CreateCommandName<LoadRecentBookCommand>() },
                MenuElementType.Separator => new SeparatorMenuElement(),
                _ => throw new NotSupportedException($"Unsupported MenuElementType: {menuNode.MenuElementType}"),
            };
        }


        // for manual
        public static List<MenuElementTableData> GetMenuTable(TreeListNode<MenuElement> node, int depth)
        {
            var list = new List<MenuElementTableData>();
            if (node.Children is null) return list;

            foreach (var child in node.Children)
            {
                if (child.Value.MenuElementType == MenuElementType.None)
                    continue;
                if (child.Value.MenuElementType == MenuElementType.Separator)
                    continue;

                list.Add(new MenuElementTableData(depth, child));

                if (child.Value.MenuElementType == MenuElementType.Group)
                {
                    list.AddRange(GetMenuTable(child, depth + 1));
                }
            }

            return list;
        }
    }
}
