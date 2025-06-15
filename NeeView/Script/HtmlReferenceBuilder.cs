using NeeLaboratory.Linq;
using NeeLaboratory.Text;
using SharpVectors.Dom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeView
{
    public partial class HtmlReferenceBuilder
    {
        [GeneratedRegex(@"[\r\n]+")]
        private static partial Regex _newLineRegex { get; }

        private readonly StringBuilder builder;

        public HtmlReferenceBuilder() : this(new StringBuilder())
        {
        }

        public HtmlReferenceBuilder(StringBuilder builder)
        {
            this.builder = builder;
        }

        public StringBuilder ToStringBuilder()
        {
            return builder;
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public HtmlReferenceBuilder Append(string src)
        {
            builder.Append(src);
            return this;
        }

        public HtmlReferenceBuilder AppendLine()
        {
            builder.AppendLine();
            return this;
        }

        /// <summary>
        /// 指定した型のリファレンスを作成する
        /// </summary>
        /// <param name="type">対象の型</param>
        /// <param name="name">型の表示面を指定。nullで標準</param>
        public HtmlReferenceBuilder Append(Type type, string? name = null)
        {
            if (type.IsEnum)
            {
                return AppendEnum(type, name);
            }
            else
            {
                return AppendClass(type, name);
            }
        }

        /// <summary>
        /// 指定したEnum型のリファレンスを作成する
        /// </summary>
        public HtmlReferenceBuilder AppendEnum(Type type, string? name)
        {
            if (!type.IsEnum) throw new ArgumentException("type must be Enum");

            var title = name ?? $"[Enum] {type.Name}";

            builder.Append(CultureInfo.InvariantCulture, $"<h3 id=\"{type.Name}\">{title}</h3>").AppendLine();

            var memberName = new DocumentMemberName(type);
            AppendSummary(memberName);

            builder.Append(CultureInfo.InvariantCulture, $"<h4>{ResourceService.GetString("@Word.Fields")}</h4>").AppendLine();

            AppendDictionary(type.VisibleAliasNameDictionary().ToDictionary(e => e.Key.ToString(), e => e.Value));

            return this;
        }

        /// <summary>
        /// DictionaryをHTMLテーブルとして出力する
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="style">table の class。nullで標準</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendDictionary(Dictionary<string, string> dictionary, string? style = null)
        {
            var attributes = style is not null ? $" class=\"{style}\"" : "";
            builder.Append(CultureInfo.InvariantCulture, $"<p><table{attributes}>").AppendLine();
            foreach (var member in dictionary)
            {
                builder.Append(CultureInfo.InvariantCulture, $"<tr><td>{member.Key}</td><td>{member.Value}</td></tr>").AppendLine();
            }
            builder.Append("</table></p>").AppendLine();

            return this;
        }

        /// <summary>
        /// DataTableをHTMLテーブルとして出力する
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="isHeader">ヘッダ行を出力するか</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendDataTable(DataTable dataTable, bool isHeader)
        {
            builder.Append(CultureInfo.InvariantCulture, $"<p><table>").AppendLine();

            if (isHeader)
            {
                builder.Append("<tr>");
                foreach (DataColumn col in dataTable.Columns)
                {
                    builder.Append(CultureInfo.InvariantCulture, $"<th>{col.Caption}</th>");
                }
                builder.Append("</tr>").AppendLine();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                builder.Append("<tr>");
                foreach (DataColumn col in dataTable.Columns)
                {
                    var attributes = (col.ColumnName == "summary") ? " class=\"td-middle\"" : "";
                    builder.Append(CultureInfo.InvariantCulture, $"<td{attributes}>{row[col]}</td>");
                }
                builder.Append("</tr>").AppendLine();
            }

            builder.Append("</table></p>").AppendLine();

            return this;
        }


        /// <summary>
        /// Summary,RemarksをHTMLに出力
        /// </summary>
        /// <param name="name">対象のリソーステキスト名</param>
        /// <param name="isRemarks">Remarksを含める</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendSummary(DocumentMemberName name, bool isRemarks = true)
        {
            var summary = name.GetHtmlDocument("");
            builder.Append($"<p>").Append(summary).Append("</p>").AppendLine();

            if (isRemarks)
            {
                var remarks = name.GetHtmlDocument(".Remarks", false);
                if (remarks != null)
                {
                    builder.Append($"<p>").Append(remarks).Append("</p>").AppendLine();
                }
            }

            return this;
        }

        private static bool IsDocumentable(MemberInfo info)
        {
            if (info.GetCustomAttribute<ObsoleteAttribute>() is not null) return false;

            var attribute = info.GetCustomAttribute<DocumentableAttribute>();
            if (attribute is null) return false;

            return attribute != null && attribute.IsEnabled && (!attribute.IsBaseClassOnly || info.DeclaringType == info.ReflectedType);
        }


        /// <summary>
        /// 指定したClass型のリファレンスを作成する
        /// </summary>
        public HtmlReferenceBuilder AppendClass(Type type, string? name = null)
        {
            var className = name ?? type.Name;
            var title = name ?? $"[Class] {type.Name}";

            builder.Append(CultureInfo.InvariantCulture, $"<h3 id=\"{type.Name}\">{title}</h3>").AppendLine();

            var properties = type.GetProperties().Where(e => IsDocumentable(e)).OrderBy(e => e.Name);
            var methods = type.GetMethods().Where(e => IsDocumentable(e)).OrderBy(e => e.Name);

            // summary
            var memberName = new DocumentMemberName(type);
            AppendSummary(memberName);

            // base class
            var attribute = type.GetCustomAttribute<DocumentableBaseClassAttribute>();
            if (attribute != null && attribute.BaseClass != type)
            {
                Debug.Assert(type.IsSubclassOf(attribute.BaseClass));
                builder.Append("<p>" + string.Format(CultureInfo.InvariantCulture, ResourceService.GetString("@_ScriptManual.ClassInheritance"), TypeToString(attribute.BaseClass)) + "</p>").AppendLine();
            }

            // derived class
            var derivedAttribute = type.GetCustomAttribute<DocumentableDerivedClassAttribute>();
            if (derivedAttribute != null && derivedAttribute.DerivedClass.Any())
            {
                Debug.Assert(derivedAttribute.DerivedClass.All(e => e.IsSubclassOf(type)));
                builder.Append("<p>" + string.Format(CultureInfo.InvariantCulture, ResourceService.GetString("@_ScriptManual.ClassDerivation"), derivedAttribute.DerivedClass.Select(e => TypeToString(e))) + "</p>").AppendLine();
            }

            // property
            if (properties.Any())
            {
                builder.Append(CultureInfo.InvariantCulture, $"<h4>{ResourceService.GetString("@Word.Properties")}</h4>").AppendLine();
                AppendDataTable(PropertiesToDataTable(properties), false);
            }

            // method
            if (methods.Any())
            {
                builder.Append(CultureInfo.InvariantCulture, $"<h4>{ResourceService.GetString("@Word.Methods")}</h4>").AppendLine();
                AppendDataTable(MethodsToDataTable(methods), false);
            }

            return this;
        }

        public HtmlReferenceBuilder CreateMethodTable(Type type, string? prefix)
        {
            var methods = type.GetMethods().Where(e => IsDocumentable(e));
            if (methods.Any())
            {
                Append($"<h4>{ResourceService.GetString("@Word.Methods")}</h4>").AppendLine();
                AppendDataTable(MethodsToDataTable(methods), false);
            }
            return this;
        }

        /// <summary>
        /// 使用例たちの出力
        /// </summary>
        private HtmlReferenceBuilder AppendExamples(IEnumerable<string> names)
        {
            var examples = names.Select(e => GetDocument(e, ".Example", false)?.Trim()).Where(e => !string.IsNullOrEmpty(e));
            if (examples.Any())
            {
                builder.Append(CultureInfo.InvariantCulture, $"<h4>{ResourceService.GetString("@Word.Example")}</h4>").AppendLine();
                foreach (var example in examples)
                {
                    builder.Append("<p><pre><code class=\"example\">");
                    builder.Append(example);
                    builder.Append("</code></pre></p>").AppendLine();
                }
            }

            return this;
        }

        /// <summary>
        /// 使用例の存在チェック
        /// </summary
        private bool ExampleExists(string name)
        {
            return GetDocument(name, ".Example", false) is not null;
        }

        /// <summary>
        /// 使用例の出力
        /// </summary>
        private HtmlReferenceBuilder AppendExample(string name)
        {
            return AppendExamples(new string[] { name });
        }

        /// <summary>
        /// パラメーターのDataTable化
        /// </summary>
        private DataTable ParametersToDataTable(MethodInfo method, IEnumerable<ParameterInfo> parameters)
        {
            var dataTable = new DataTable("Parameters");
            dataTable.Columns.Add(new DataColumn("name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("type", typeof(string)));
            dataTable.Columns.Add(new DataColumn("summary", typeof(string)));

            var memberName = new DocumentMemberName(method);

            foreach (var parameter in parameters)
            {
                var typeString = TypeToString(parameter.ParameterType);
                var summary = memberName.GetHtmlDocumentWithRemarks(parameter.Name?.ToTitleCase());
                dataTable.Rows.Add(parameter.Name, typeString, summary);
            }

            return dataTable;
        }

        /// <summary>
        /// プロパティのDataTable化
        /// </summary>
        private DataTable PropertiesToDataTable(IEnumerable<PropertyInfo> properties)
        {
            var dataTable = new DataTable("Properties");
            dataTable.Columns.Add(new DataColumn("name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("type", typeof(string)));
            dataTable.Columns.Add(new DataColumn("rw", typeof(string)));
            dataTable.Columns.Add(new DataColumn("summary", typeof(string)));

            foreach (var property in properties)
            {
                var name = property.DeclaringType?.Name + "." + property.Name;
                var attribute = property.GetCustomAttribute<DocumentableAttribute>();
                var returnTypeAttribute = property.GetCustomAttribute<ReturnTypeAttribute>();
                var typeString = TypesToString(returnTypeAttribute?.ReturnTypes, property.PropertyType) + (attribute?.DocumentType != null ? $" ({TypeToString(attribute.DocumentType)})" : "");
                var rw = (property.CanRead ? "r" : "") + (property.CanWrite ? "w" : "");
                var memberName = new DocumentMemberName(property);
                var summary = memberName.GetHtmlDocumentWithRemarks() + CreatePropertyDetail(property);
                dataTable.Rows.Add(property.Name, typeString, rw, summary);
            }

            return dataTable;
        }

        private string TypesToString(Type[]? types, Type defaultType)
        {
            if (types is null)
            {
                return TypeToString(defaultType);
            }
            return string.Join("<br/>", types.Select(e => TypeToString(e)));
        }

        private string CreatePropertyDetail(PropertyInfo property)
        {
            var name = property.DeclaringType?.Name + "." + property.Name;
            if (!ExampleExists(name)) return "";

            var builder = new HtmlReferenceBuilder();
            builder.Append("<details>");
            builder.AppendExample(name);
            builder.Append("</details>");
            return builder.ToString();
        }

        /// <summary>
        /// メソッドのDataTable化
        /// </summary>
        private DataTable MethodsToDataTable(IEnumerable<MethodInfo> methods)
        {
            var dataTable = new DataTable("Methods");
            dataTable.Columns.Add(new DataColumn("name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("return", typeof(string)));
            dataTable.Columns.Add(new DataColumn("summary", typeof(string)));

            foreach (var method in methods)
            {
                var name = method.DeclaringType?.Name + "." + method.Name;
                var attribute = method.GetCustomAttribute<DocumentableAttribute>() ?? throw new InvalidOperationException();
                var title = (attribute.Name ?? method.Name) + "(" + string.Join(", ", method.GetParameters().Select(e => TypeToString(e.ParameterType))) + ")";
                var returnTypeAttribute = method.GetCustomAttribute<ReturnTypeAttribute>();
                var typeString = TypesToString(returnTypeAttribute?.ReturnTypes, method.ReturnType) + (attribute?.DocumentType != null ? $" ({TypeToString(attribute.DocumentType)})" : "");
                var memberName = new DocumentMemberName(method);
                var summary = memberName.GetHtmlDocumentWithRemarks() + CreateMethodDetail(method);

                dataTable.Rows.Add(title, typeString, summary);
            }

            return dataTable;
        }

        private string CreateMethodDetail(MethodInfo method)
        {
            var name = method.DeclaringType?.Name + "." + method.Name;
            var parameters = method.GetParameters();
            if (parameters.Length <= 0 && method.ReturnType == typeof(void) && !ExampleExists(name)) return "";

            var builder = new HtmlReferenceBuilder();

            builder.Append("<details>");

            if (parameters.Length > 0)
            {
                builder.Append($"<h4>{ResourceService.GetString("@Word.Parameters")}</h4>").AppendLine();
                builder.AppendDataTable(ParametersToDataTable(method, parameters), false);
            }

            var attribute = method.GetCustomAttribute<DocumentableAttribute>();
            Debug.Assert(attribute != null);

            if (method.ReturnType != typeof(void))
            {
                builder.Append($"<h4>{ResourceService.GetString("@Word.Returns")}</h4>").AppendLine();
                var returnTypeAttribute = method.GetCustomAttribute<ReturnTypeAttribute>();
                var typeString = TypesToString(returnTypeAttribute?.ReturnTypes, method.ReturnType) + (attribute?.DocumentType != null ? $" ({TypeToString(attribute.DocumentType)})" : "");
                var memberName = new DocumentMemberName(method);
                var summary = memberName.GetHtmlDocument(".Returns") ?? "";
                builder.AppendDictionary(new Dictionary<string, string> { [typeString] = summary });
            }

            builder.AppendExample(name);

            builder.Append("</details>");

            return builder.ToString();
        }

        /// <summary>
        /// プレーンテキストのHTML化
        /// </summary>
        /// <remarks>
        /// 改行変換だけの簡単なもの
        /// </remarks>
        private static string? TextToHtmlFormat(string? src)
        {
            if (src is null) return null;

            return _newLineRegex.Replace(src, "<br />");
        }

        /// <summary>
        /// リソースキーから文字列を取得する
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="postfix">リソース属性名 (e.g. #Remarks)</param>
        /// <param name="notNull">trueの場合、リソースが存在しなければリソース名を返す</param>
        /// <returns>取得された文字列</returns>
        private static string? GetDocument(string name, string? postfix, bool notNull = true)
        {
            var resourceId = "@" + JoinName(name, postfix);
            var text = ResourceService.GetResourceString(resourceId, true);
            if (text is null && notNull)
            {
                text = resourceId;
            }
            return text;
        }

        private static string JoinName(string s1, string? s2)
        {
            if (string.IsNullOrEmpty(s2)) return s1;

            return s1 + (s2.StartsWith('.') ? "" : ".") + s2;
        }

        /// <summary>
        /// name + name.Remarks の HTML 文字列を作成する
        /// </summary>
        /// <param name="name"></param>
        /// <param name="postfix"></param>
        /// <param name="notNull"></param>
        /// <returns></returns>
        private static string? GetHtmlDocumentWithRemarks(string name, bool notNull = true)
        {
            var text = GetDocument(name, "", notNull);
            if (text is null) return null;
            var remarks = GetDocument(name, ".Remarks", false);
            if (remarks is not null)
            {
                text += "\r\n" + remarks;
            }
            return TextToHtmlFormat(text);
        }

        /// <summary>
        /// リソースキーからHTML文字列を取得する
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="postfix">リソース属性名 (e.g. #Remarks)</param>
        /// <param name="notNull">trueの場合、リソースが存在しなければリソース名を返す</param>
        /// <returns>HTML化された文字列</returns>
        private static string? GetHtmlDocument(string name, string? postfix, bool notNull = true)
        {
            var text = GetDocument(name, postfix, notNull);
            return TextToHtmlFormat(text);
        }

        /// <summary>
        /// リファレンスに適した型名を取得する
        /// </summary>
        private string TypeToString(Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }

            if (type.IsEnum)
            {
                return TypeAnchor(type);
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType is null) throw new InvalidOperationException();
                var elementTypeString = TypeToString(elementType);
                return elementTypeString + "[]";
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return "int";
                case TypeCode.Double:
                    return "double";
                case TypeCode.String:
                    return "string";
                case TypeCode.DateTime:
                    return "Date";
            }

            if (type == typeof(object))
            {
                return "object";
            }

            var propertyIndexer = type.GetProperties().Where(p => p.GetIndexParameters().Length != 0).FirstOrDefault();
            if (propertyIndexer != null)
            {
                if (propertyIndexer.PropertyType != typeof(object) && propertyIndexer.PropertyType != typeof(string))
                {
                    return ToAnchor(GetFixedTypeName(propertyIndexer.PropertyType)) + "[]";
                }

                return "dictionary";
            }

            return TypeAnchor(type);
        }

        /// <summary>
        /// 属性による名前指定を反映
        /// </summary>
        private static string GetFixedTypeName(Type type)
        {
            var attribute = type.GetCustomAttribute<DocumentableAttribute>();
            if (attribute != null && attribute.Name != null)
            {
                return attribute.Name;
            }
            else
            {
                return type.Name;
            }
        }

        /// <summary>
        /// 型をアンカー付きテキストに変換
        /// </summary>
        private static string TypeAnchor(Type type)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType is null) throw new InvalidOperationException();
                return ToAnchor(GetFixedTypeName(elementType)) + "[]";
            }
            else
            {
                return ToAnchor(GetFixedTypeName(type));
            }
        }

        /// <summary>
        /// 文字列のアンカー化
        /// </summary>
        /// <param name="src"></param>
        /// <param name="id">参照先。nullで標準</param>
        private static string ToAnchor(string src, string? id = null)
        {
            id = id ?? "#" + src;
            return $"<a href=\"{id}\">{src}</a>";
        }


        private class DocumentMemberName
        {
            private readonly DocumentableAttribute? _attribute;
            private readonly Type? _type;
            private readonly string? _member;

            public DocumentMemberName(Type type)
            {
                _type = type;
                _member = null;
                _attribute = type.GetCustomAttribute<DocumentableAttribute>();
            }

            public DocumentMemberName(MemberInfo memberInfo)
            {
                _type = memberInfo.DeclaringType;
                _member = memberInfo.Name;
                _attribute = memberInfo.GetCustomAttribute<DocumentableAttribute>() ?? throw new ArgumentException("DocumentableAttribute is required.");
            }

            public bool HasAltName => _attribute?.HasAltName() ?? false;

            private string JoinName(params string?[] names)
            {
                return string.Join(".", names.WhereNotNull());
            }

            public string GetName()
            {
                return JoinName(_type?.Name, _member);
            }

            public string GetAltName()
            {
                if (_attribute != null && _attribute.AltName != null && _attribute.AltName.StartsWith("@", StringComparison.Ordinal))
                {
                    return _attribute.AltName[1..];
                }
                else
                {
                    return (_attribute?.AltClassType ?? _type)?.Name + "." + (_attribute?.AltName ?? _member);
                }
            }

            public string GetAltParameterName(DocumentableAttribute attr)
            {
                if (attr.AltName != null && attr.AltName.StartsWith("@", StringComparison.Ordinal))
                {
                    return attr.AltName[1..];
                }
                else
                {
                    return (attr.AltClassType ?? _attribute?.AltClassType ?? _type)?.Name + "." + (attr.AltName ?? _attribute?.AltName ?? _member);
                }
            }

            public string? GetHtmlDocument(string? postfix = null, bool notNull = true)
            {
                var summary = HtmlReferenceBuilder.GetHtmlDocument(GetName(), postfix, false);
                if (summary is null)
                {
                    summary = HtmlReferenceBuilder.GetHtmlDocument(GetAltName(), postfix, false);
                }
                return summary ?? (notNull ? "@" + JoinName(GetName(), postfix) : null);
            }

            public string? GetHtmlDocumentWithRemarks(string? postfix = null, bool notNull = true)
            {
                var summary = HtmlReferenceBuilder.GetHtmlDocumentWithRemarks(JoinName(GetName(), postfix), false);
                if (summary is null)
                {
                    summary = HtmlReferenceBuilder.GetHtmlDocumentWithRemarks(JoinName(GetAltName(), postfix), false);
                }
                return summary ?? (notNull ? "@" + JoinName(GetName(), postfix) : null);
            }

            public string? GetHtmlDocumentWithRemarks(ParameterInfo parameterInfo)
            {
                var attr = parameterInfo.GetCustomAttribute<DocumentableAttribute>();
                var postfix = parameterInfo.Name?.ToTitleCase();
                if (attr is null) return GetHtmlDocumentWithRemarks(postfix);

                var summary = HtmlReferenceBuilder.GetHtmlDocumentWithRemarks(HtmlReferenceBuilder.JoinName(GetName(), postfix), false);
                if (summary is not null) return summary;

                summary = HtmlReferenceBuilder.GetHtmlDocumentWithRemarks(HtmlReferenceBuilder.JoinName(GetAltParameterName(attr), postfix), true);
                if (summary is not null) return summary;

                summary = HtmlReferenceBuilder.GetHtmlDocumentWithRemarks(HtmlReferenceBuilder.JoinName(GetAltName(), postfix), true);
                return summary ?? "";
            }
        }
    }
}
