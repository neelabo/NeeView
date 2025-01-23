using System;
using System.Globalization;
using System.Reflection;

namespace NeeView.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionMemberAttribute : OptionBaseAttribute
    {
        public string? ShortName;
        public string? LongName;
        public string? Default;
        public bool HasParameter;
        public bool RequireParameter;
        public bool IsVisible = true;

        public OptionMemberAttribute() { }
        public OptionMemberAttribute(string? shortName, string? longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }


    public class OptionMemberElement : IComparable<OptionMemberElement>
    {
        public string? LongName => _attribute.LongName;
        public string? ShortName => _attribute.ShortName;
        public string? Default => _attribute.Default;
        public bool HasParameter => _attribute.HasParameter;
        public bool RequireParameter => _attribute.RequireParameter;
        public string? HelpText => ResourceService.GetString(_attribute.HelpText);
        public bool IsVisible => _attribute.IsVisible;

        public string PropertyName => _info.Name;

        private readonly PropertyInfo _info;
        private readonly OptionMemberAttribute _attribute;


        public OptionMemberElement(PropertyInfo info, OptionMemberAttribute attribute)
        {
            _info = info;
            _attribute = attribute;
        }

        /// <summary>
        /// オプション引数指定可能値を取得
        /// ヘルプ用
        /// </summary>
        /// <returns></returns>
        public string GetValuePrototype()
        {
            if (_info.PropertyType.IsEnum)
            {
                return string.Join("|", Enum.GetNames(_info.PropertyType));
            }

            Type? nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                return string.Join("|", Enum.GetNames(nullable));
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            return typeCode switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.String => "string",
                TypeCode.Int32 => "number",
                TypeCode.Double => "number",
                _ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.NotSupportType"), _info.PropertyType)),
            };
        }

        //
        public void SetValue(object _source, string value)
        {
            if (_info.PropertyType.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(_info.PropertyType, value));
                return;
            }

            Type? nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(nullable, value));
                return;
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    _info.SetValue(_source, bool.Parse(value));
                    break;
                case TypeCode.String:
                    _info.SetValue(_source, value);
                    break;
                case TypeCode.Int32:
                    _info.SetValue(_source, int.Parse(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Double:
                    _info.SetValue(_source, double.Parse(value, CultureInfo.InvariantCulture));
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.NotSupportType"), _info.PropertyType.Name));
            }
        }

        public int CompareTo(OptionMemberElement? other)
        {
            if (other == null)
            {
                return 1;
            }
            else if (ShortName is not null)
            {
                return other.ShortName is null ? -1 : string.Compare(ShortName, other.ShortName, StringComparison.Ordinal);
            }
            else if (other.ShortName is not null)
            {
                return 1;
            }
            else if (LongName is not null)
            {
                return other.LongName is null ? -1 : string.Compare(LongName, other.LongName, StringComparison.Ordinal);
            }
            else if (other.LongName is not null)
            {
                return 1;
            }
            return 0;
        }
    }
}
