using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessCollection : TreeCollection<IQuickAccessEntry>
    {
        static QuickAccessCollection() => Current = new QuickAccessCollection();
        public static QuickAccessCollection Current { get; }


        public QuickAccessCollection() : base(new QuickAccessRoot())
        {
            Root.IsExpanded = true;
        }


        public TreeListNode<IQuickAccessEntry>? FindNode(QueryPath query)
        {
            if (query.Scheme != QueryScheme.QuickAccess)
            {
                throw new ArgumentException("Not a QuickAccess scheme path.", nameof(query));
            }

            if (query.Path == null)
            {
                return Root;
            }

            return FindNode(Root, query.Path);
        }

        // TODO: TreeCollection に移動
        private TreeListNode<IQuickAccessEntry>? FindNode(TreeListNode<IQuickAccessEntry> node, string path)
        {
            return FindNode(node, path.Split(LoosePath.Separators));
        }

        private TreeListNode<IQuickAccessEntry>? FindNode(TreeListNode<IQuickAccessEntry> node, IEnumerable<string> pathTokens)
        {
            if (pathTokens == null)
            {
                return null;
            }

            if (!pathTokens.Any())
            {
                return node;
            }

            var name = pathTokens.First();
            var child = node.Children.FirstOrDefault(e => e.Value.Name == name);
            if (child != null)
            {
                return FindNode(child, pathTokens.Skip(1));
            }

            return null;
        }


        public TreeListNode<IQuickAccessEntry>? AddNewFolder(TreeListNode<IQuickAccessEntry> parent, string? name)
        {
            return InsertNewFolder(parent, parent.Children.Count, name);
        }

        public TreeListNode<IQuickAccessEntry>? InsertNewFolder(TreeListNode<IQuickAccessEntry> parent, int index, string? name)
        {
            if (parent.Value is not QuickAccessFolder) return null;

            var ignoreNames = parent.Children.Where(e => e.Value is IQuickAccessEntry).Select(e => e.Value.Name).WhereNotNull();
            var validName = GetValidateFolderName(ignoreNames, name, Properties.TextResources.GetString("Word.NewFolder"));
            var node = new TreeListNode<IQuickAccessEntry>(new QuickAccessFolder() { Name = validName });

            Insert(parent, index, node);
            parent.IsExpanded = true;
            return node;
        }

        private static string GetValidateFolderName(IEnumerable<string> names, string? name, string defaultName)
        {
            name = BookmarkTools.GetValidateName(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = defaultName;
            }
            if (names.Contains(name))
            {
                int count = 1;
                string newName;
                do
                {
                    newName = $"{name} ({++count})";
                }
                while (names.Contains(newName));
                name = newName;
            }

            return name;
        }


        #region Memento
        [Memento]
        public class Memento
        {
            public List<QuickAccessTreeNode>? Items { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Items = QuickAccessTreeNodeConverter.ConvertFrom(Root).Children;
            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;
            if (memento.Items is null) return;

            var items = memento.Items.Select(e => QuickAccessTreeNodeConverter.ConvertToTreeListNode(e)).WhereNotNull().ToList();
            Reset(items);
        }

        #endregion

    }



    public class QuickAccessTreeNode
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Path { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<QuickAccessTreeNode>? Children { get; set; }

        public bool IsFolder => Children != null;
    }


    public static class QuickAccessTreeNodeConverter
    {
        public static QuickAccessTreeNode ConvertFrom(TreeListNode<IQuickAccessEntry> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var node = new QuickAccessTreeNode();

            if (source.Value is QuickAccessFolder folder)
            {
                node.Name = folder.Name;
                node.Children = new List<QuickAccessTreeNode>();
                foreach (var child in source.Children)
                {
                    node.Children.Add(ConvertFrom(child));
                }
            }
            else if (source.Value is QuickAccess quickAccess)
            {
                node.Name = quickAccess.RawName;
                node.Path = quickAccess.Path;
            }
            else
            {
                throw new NotSupportedException();
            }

            return node;
        }

        public static TreeListNode<IQuickAccessEntry>? ConvertToTreeListNode(QuickAccessTreeNode source)
        {
            if (source.IsFolder)
            {
                var folder = new QuickAccessFolder()
                {
                    Name = source.Name
                };

                var node = new TreeListNode<IQuickAccessEntry>(folder);
                if (source.Children is not null)
                {
                    foreach (var child in source.Children)
                    {
                        var childNode = ConvertToTreeListNode(child);
                        if (childNode is not null)
                        {
                            node.Add(childNode);
                        }
                    }
                }
                return node;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(source.Path))
                {
                    return null;
                }
                var quickAccess = new QuickAccess(source.Path)
                {
                    Name = source.Name ?? "",
                };
                var node = new TreeListNode<IQuickAccessEntry>(quickAccess);
                return node;
            }
        }
    }
}
