using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// ContextMenu 保存用データ構造
    /// </summary>
    public class MenuNode
    {
        public MenuNode(string? name, MenuElementType menuElementType, string? commandName)
        {
            Name = name;
            MenuElementType = menuElementType;
            CommandName = commandName;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        public MenuElementType MenuElementType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CommandName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MenuNode>? Children { get; set; }


        public IEnumerable<MenuNode> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var subChild in child.GetEnumerator())
                    {
                        yield return subChild;
                    }
                }
            }
        }
    }
}
