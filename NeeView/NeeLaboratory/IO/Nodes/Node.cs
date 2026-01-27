using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeeLaboratory.IO.Nodes
{
    public class Node
    {
        public Node(string name)
        {
            Name = name.TrimEnd('\\');

            // NOTE: ネットワークパス用に先頭の区切り文字を許容する
            if (Name.TrimStart('\\').Contains("\\", StringComparison.Ordinal)) throw new ArgumentException("It contains a delimiter: '\\'");
        }

        public string Name { get; set; }

        public Node? Parent { get; set; }

        public List<Node>? Children { get; set; }

        public string FullName => Path.Combine(Parent?.FullName ?? "", Name);

        public object? Content { get; set; }


        public bool HasParent(Node node)
        {
            return Parent != null && (Parent == node || Parent.HasParent(node));
        }

        /// <summary>
        /// 子ノード全削除
        /// </summary>
        public void ClearChildren()
        {
            if (Children is null) return;
            foreach (var child in Children)
            {
                child.Parent = null;
            }
            Children = null;
        }

        /// <summary>
        /// ノード検索
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Node? FindChild(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException();

            if (Children is null) return null;
            return Children.FirstOrDefault(e => e.Name == name);
        }

        /// <summary>
        /// ノード追加
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node AddChild(Node node)
        {
            if (node.Parent is not null)
            {
                node.Parent.RemoveChild(node);
                Debug.Assert(node.Parent is null);
            }

            if (Children is null)
            {
                Children = new List<Node>();
            }

            this.Children.Add(node);
            node.Parent = this;
            return node;
        }

        /// <summary>
        /// ノード追加
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>追加されたノード</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> が空</exception>
        /// <exception cref="InvalidOperationException">既に登録されている</exception>
        public Node AddChild(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException($"Argument is empty: {nameof(name)}");
            if (FindChild(name) != null) throw new ArgumentException($"Already exists: {nameof(name)}");

            return AddChild(new Node(name));
        }

        /// <summary>
        /// ノード削除
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node? RemoveChild(Node node)
        {
            if (node.Parent != this) return null;

            node.Parent = null;
            Children?.Remove(node);
            return node;
        }

        /// <summary>
        /// ノード削除
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Node? RemoveChild(string name)
        {
            var node = FindChild(name);
            if (node is null) return null;

            return RemoveChild(node);
        }

        public IEnumerable<Node> WalkChildren()
        {
            if (Children is not null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.Walk())
                    {
                        yield return node;
                    }
                }
            }
        }

        public IEnumerable<Node> Walk()
        {
            yield return this;

            if (Children is not null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.Walk())
                    {
                        yield return node;
                    }
                }
            }
        }

        public override string ToString()
        {
            return FullName;
        }


        /// <summary>
        /// 開発用：ツリー出力
        /// </summary>
        /// <param name="level"></param>
        [Conditional("DEBUG")]
        public void Dump(int level = 0)
        {
            var text = new string(' ', level * 4) + Name;
            Debug.WriteLine(text);
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Dump(level + 1);
                }
            }

            ////Logger.Trace($"{Path}:({AllNodes.Count()})");
        }

    }
}
