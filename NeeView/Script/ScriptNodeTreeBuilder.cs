using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public static class ScriptNodeTreeBuilder
    {
        public static ScriptNodeDirect Create(object source, string name)
        {
            var root = new ScriptNodeDirect(source, name);
            root.Children = CreateChildren(root);
            return root;
        }

        private static List<ScriptNode>? CreateChildren(ScriptNode node)
        {
            var type = node.Type;

            var children = new List<ScriptNode>();

            if (!node.Type.IsClass)
            {
                return null;
            }

            if (node.Type == typeof(string))
            {
                return null;
            }

            // 配列は非対応
            if (node.Type.IsArray)
            {
                return null;
            }

            // Genericは非対応
            if (node.Type.IsGenericType)
            {
                return null;
            }

            // Obsoleteは非対応
            if (node.Obsolete != null)
            {
                return null;
            }

            if (node.Value is null)
            {
                return null;
            }

            if (node.Value is PropertyMap propertyMap)
            {
                foreach (var item in propertyMap)
                {
                    var child = new ScriptNodeReflection(new ScriptNodeReflectionSource(propertyMap, new ScriptMemberInfo(item.Key)));
                    child.Children = CreateChildren(child);
                    children.Add(child);
                }
            }

            if (node.Value is CommandAccessorMap commandMap)
            {
                foreach (var item in commandMap)
                {
                    var child = new ScriptNodeReflection(new ScriptNodeReflectionSource(commandMap, new ScriptMemberInfo(item.Key)));
                    child.Children = CreateChildren(child);
                    children.Add(child);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                var child = new ScriptNodeReflection(new ScriptNodeReflectionSource(node.Value, new ScriptMemberInfo(property)));
                child.Children = CreateChildren(child);
                children.Add(child);
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                var child = new ScriptNodeReflection(new ScriptNodeReflectionSource(node.Value, new ScriptMemberInfo(method)));
                children.Add(child);
            }

            return children;

            static bool IsDocumentable(MemberInfo info)
            {
                return info.GetCustomAttribute<DocumentableAttribute>() != null;
            }
        }
    }



    /// <summary>
    /// ScriptNode 情報表示用
    /// </summary>
    /// <remarks>
    /// FullNameが取得できるのがScriptNodeとの違い
    /// </remarks>
    public class ScriptNodeUnit
    {
        public ScriptNodeUnit(string? prefix, ScriptNode node)
        {
            Prefix = prefix;
            Node = node;
        }

        public string? Prefix { get; }
        public ScriptNode Node { get; }

        public string Name => Node.Name;
        public string FullName => CreateFullName();

        public ScriptErrorLevel ErrorLevel => Node.Alternative?.ErrorLevel ?? ScriptErrorLevel.Error;

        public string Alternative => CreateAlternativeMessage();

        private string CreateAlternativeMessage()
        {
            string msg = "";
            if (Node.Alternative?.Alternative is not null)
            {
                var alt = Node.Alternative;
                msg = alt.IsFullName ? alt.Alternative : Prefixed(alt.Alternative);
            }
            return string.IsNullOrEmpty(msg) ? "x" : msg;
        }

        private string CreateFullName()
        {
            var name = Prefixed(Name);
            if (Node.Category == ScriptMemberInfoType.Method)
            {
                name += "()";
            }
            return name;
        }

        private string Prefixed(string s)
        {
            return Prefix != null ? Prefix + "." + s : s;
        }
    }



    public abstract class ScriptNode
    {
        public abstract string Name { get; }
        public virtual ScriptMemberInfoType Category => ScriptMemberInfoType.None;
        public abstract Type Type { get; }
        public abstract object? Value { get; }
        public virtual ObsoleteAttribute? Obsolete => null;
        public virtual AlternativeAttribute? Alternative => null;
        public List<ScriptNode>? Children { get; set; }


        public IEnumerable<ScriptNodeUnit> GetUnitEnumerator(string? prefix)
        {
            var parent = new ScriptNodeUnit(prefix, this);
            yield return parent;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.GetUnitEnumerator(parent.FullName))
                    {
                        yield return node;
                    }
                }
            }
        }
    }



    /// <summary>
    /// ScriptNode: sourceをそのまま参照するタイプ
    /// </summary>
    public class ScriptNodeDirect : ScriptNode
    {
        private readonly object _source;

        public ScriptNodeDirect(object source, string name)
        {
            _source = source;
            Name = name;
        }

        public override string Name { get; }
        public override Type Type => _source.GetType();
        public override object Value => _source;
    }


    /// <summary>
    /// ScriptNode: sourceを基準にリフレクションでプロパティ等のメンバーを参照するタイプ
    /// </summary>
    public class ScriptNodeReflection : ScriptNode
    {
        private readonly ScriptNodeReflectionSource _source;

        public ScriptNodeReflection(ScriptNodeReflectionSource source)
        {
            _source = source;
        }

        public override string Name => _source.GetName();
        public override ScriptMemberInfoType Category => _source.MemberInfo.Type;
        public override Type Type => _source.GetValueType();
        public override object? Value => _source.GetValue();
        public override ObsoleteAttribute? Obsolete => _source.GetValueObsolete();
        public override AlternativeAttribute? Alternative => _source.GetValueAlternative();
    }



    /// <summary>
    /// リフレクション参照ソース
    /// </summary>
    /// <remarks>
    /// PropertyMapやCommandAccessorMapも対応
    /// </remarks>
    public class ScriptNodeReflectionSource
    {
        public ScriptNodeReflectionSource(object source, ScriptMemberInfo memberInfo)
        {
            Source = source;
            MemberInfo = memberInfo;
        }


        public object Source { get; set; }
        public ScriptMemberInfo MemberInfo { get; set; }


        public string GetName()
        {
            return MemberInfo.Type switch
            {
                ScriptMemberInfoType.Property => MemberInfo.PropertyInfo.Name,
                ScriptMemberInfoType.Method => MemberInfo.MethodInfo.Name,
                ScriptMemberInfoType.IndexKey => MemberInfo.IndexKey,
                _ => throw new NotSupportedException(),
            };
        }

        public ObsoleteAttribute? GetValueObsolete()
        {
            return MemberInfo.Type switch
            {
                ScriptMemberInfoType.Property => MemberInfo.PropertyInfo.GetCustomAttribute<ObsoleteAttribute>(),
                ScriptMemberInfoType.Method => MemberInfo.MethodInfo.GetCustomAttribute<ObsoleteAttribute>(),
                ScriptMemberInfoType.IndexKey => GetIndexerValueObsolete(Source, MemberInfo.IndexKey),
                _ => throw new NotSupportedException(),
            };
        }

        public AlternativeAttribute? GetValueAlternative()
        {
            return MemberInfo.Type switch
            {
                ScriptMemberInfoType.Property => MemberInfo.PropertyInfo.GetCustomAttribute<AlternativeAttribute>(),
                ScriptMemberInfoType.Method => MemberInfo.MethodInfo.GetCustomAttribute<AlternativeAttribute>(),
                ScriptMemberInfoType.IndexKey => GetIndexerValueAlternative(Source, MemberInfo.IndexKey),
                _ => throw new NotSupportedException(),
            };
        }

        public Type GetValueType()
        {
            return MemberInfo.Type switch
            {
                ScriptMemberInfoType.Property => MemberInfo.PropertyInfo.PropertyType,
                ScriptMemberInfoType.Method => MemberInfo.MethodInfo.ReturnType,
                ScriptMemberInfoType.IndexKey => GetIndexerValueType(Source, MemberInfo.IndexKey),
                _ => throw new NotSupportedException(),
            };
        }

        public object? GetValue()
        {
            return MemberInfo.Type switch
            {
                ScriptMemberInfoType.Property => MemberInfo.PropertyInfo.GetValue(Source),
                ScriptMemberInfoType.IndexKey => GetIndexerValue(Source, MemberInfo.IndexKey),
                _ => throw new NotSupportedException(),
            };
        }

        private static ObsoleteAttribute? GetIndexerValueObsolete(object source, string key)
        {
            return source switch
            {
                PropertyMap propertyMap => GetPropertyMapValueObsolete(propertyMap, key),
                CommandAccessorMap commandMap => GetCommandMapValueObsolete(commandMap, key),
                _ => throw new NotSupportedException(),
            };
        }

        private static AlternativeAttribute? GetIndexerValueAlternative(object source, string key)
        {
            return source switch
            {
                PropertyMap propertyMap => GetPropertyMapValueAlternative(propertyMap, key),
                CommandAccessorMap commandMap => GetCommandMapValueAlternative(commandMap, key),
                _ => throw new NotSupportedException(),
            };
        }

        private static Type GetIndexerValueType(object source, string key)
        {
            return source switch
            {
                PropertyMap propertyMap => GetPropertyMapValueType(propertyMap, key),
                CommandAccessorMap commandMap => GetCommandMapValueType(commandMap, key),
                _ => throw new NotSupportedException(),
            };
        }

        private static object? GetIndexerValue(object source, string key)
        {
            return source switch
            {
                PropertyMap propertyMap => GetPropertyMapValue(propertyMap, key),
                CommandAccessorMap commandMap => GetCommandMapValue(commandMap, key),
                _ => throw new NotSupportedException(),
            };
        }

        private static ObsoleteAttribute? GetPropertyMapValueObsolete(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            return node.Obsolete;
        }

        private static AlternativeAttribute? GetPropertyMapValueAlternative(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            return node.Alternative;
        }

        private static Type GetPropertyMapValueType(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            return node switch
            {
                PropertyMap _ => typeof(PropertyMap),
                PropertyMapSource propertySource => propertySource.PropertyInfo.PropertyType,
                _ => throw new NotSupportedException(),
            };
        }

        private static object? GetPropertyMapValue(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            return node switch
            {
                PropertyMap propertyMap_ => propertyMap_,
                PropertyMapSource propertySource => propertySource.GetValue(),
                _ => throw new NotSupportedException(),
            };
        }


        private static ObsoleteAttribute? GetCommandMapValueObsolete(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap.GetObsolete(key);
        }

        private static AlternativeAttribute? GetCommandMapValueAlternative(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap.GetAlternative(key);
        }

        private static Type GetCommandMapValueType(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            var command = commandMap.GetCommand(key);
            return command.GetType();
        }

        private static object? GetCommandMapValue(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap[key];
        }
    }



    /// <summary>
    /// リフレクションのメンバー情報
    /// </summary>
    public class ScriptMemberInfo
    {
        private readonly PropertyInfo? _propertyInfo;
        private readonly MethodInfo? _methodInfo;
        private readonly string? _indexKey;


        public ScriptMemberInfo(PropertyInfo propertyInfo)
        {
            Type = ScriptMemberInfoType.Property;
            _propertyInfo = propertyInfo;
        }

        public ScriptMemberInfo(MethodInfo methodInfo)
        {
            Type = ScriptMemberInfoType.Method;
            _methodInfo = methodInfo;
        }

        public ScriptMemberInfo(string key)
        {
            Type = ScriptMemberInfoType.IndexKey;
            _indexKey = key;
        }


        public ScriptMemberInfoType Type { get; }

        public PropertyInfo PropertyInfo
            => (Type == ScriptMemberInfoType.Property ? _propertyInfo : null) ?? throw new InvalidOperationException();

        public MethodInfo MethodInfo
            => (Type == ScriptMemberInfoType.Method ? _methodInfo : null) ?? throw new InvalidOperationException();

        public string IndexKey
            => (Type == ScriptMemberInfoType.IndexKey ? _indexKey : null) ?? throw new InvalidOperationException();
    }


    /// <summary>
    /// リフレクションのメンバー情報の種類
    /// </summary>
    public enum ScriptMemberInfoType
    {
        None,
        Property,
        Method,
        IndexKey,
    }
}
