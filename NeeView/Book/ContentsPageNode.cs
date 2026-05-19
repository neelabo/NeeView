using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeeView
{
    public partial class ContentsPageNode : ObservableObject
    {
        public ContentsPageNode()
        {
        }

        public bool IsRoot { get; set; }

        public string Name { get; set; } = "";

        [NotNull]
        public string? Title
        {
            get => field ?? Name;
            set;
        }

        public Page? Page { get; set; }

        public List<ContentsPageNode>? Children { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        private IEnumerable<ContentsPageNode> Walk()
        {
            yield return this;
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var subChild in child.Walk())
                    {
                        yield return subChild;
                    }
                }
            }
        }

        /// <summary>
        /// パスを指定してノードを追加
        /// </summary>
        public ContentsPageNode Add(Page page, string path)
        {
            // パスを分割（空の要素を除去）
            var parts = LoosePath.Split(path);

            var currentNode = this;

            foreach (var part in parts)
            {
                // Childrenがnullの場合は初期化
                currentNode.Children ??= new List<ContentsPageNode>();

                // 現在の階層に同じ名前の子ノードがあるか探す
                var child = currentNode.Children.FirstOrDefault(n => n.Name == part);

                if (child == null)
                {
                    // なければ新しく作成して追加
                    child = new ContentsPageNode { Name = part, Page = page };
                    currentNode.Children.Add(child);
                }

                // 次の階層へ移動
                currentNode = child;
            }

            return currentNode;
        }

        [Conditional("DEBUG")]
        public void Dump(int depth = 0)
        {
            var indent = new string(' ', depth * 2);
            Debug.WriteLine(indent + $"{Name}: {Page?.EntryName}");

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Dump(depth + 1);
                }
            }
        }

    }
}
