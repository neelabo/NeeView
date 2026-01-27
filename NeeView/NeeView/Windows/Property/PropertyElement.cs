using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    // 基底クラス
    public class PropertyDrawElement
    {
    }

    /// <summary>
    /// タイトル項目
    /// </summary>
    public class PropertyTitleElement : PropertyDrawElement
    {
        public string Name { get; set; }

        public PropertyTitleElement(string name)
        {
            Name = name;
        }
    }


    public class PropertyValueSource : IValueSetter
    {
        private readonly object _source;
        private readonly PropertyInfo _info;

        public PropertyValueSource(object source, PropertyInfo? info)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            _source = source;
            _info = info;

            if (source is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _info.Name)
                    {
                        ValueChanged?.Invoke(s, e);
                    }
                };
            }
        }

        public PropertyValueSource(object source, string propertyName) : this(source, source.GetType().GetProperty(propertyName))
        {
        }


        public event EventHandler? ValueChanged;


        public string Name => _info.Name;


        public void SetValue(object? value)
        {
            _info.SetValue(_source, value);
        }

        public object? GetValue()
        {
            return _info.GetValue(_source);
        }
    }


    /// <summary>
    /// プロパティ項目表示編集
    /// </summary>
    public class PropertyMemberElement : PropertyDrawElement, IValueSetter
    {
        private PropertyInfo _info;


        public PropertyMemberElement(object source, PropertyInfo info, PropertyMemberAttribute attribute, PropertyMemberElementOptions options)
        {
            InitializeCommon(source, info, attribute, options);

            switch (attribute)
            {
                case PropertyPercentFontSizeAttribute percentFontSizeAttribute:
                    InitializeByPercentFontSizeAttribute(percentFontSizeAttribute);
                    break;

                case PropertyPercentAttribute percentAttribute:
                    InitializeByPercentAttribute(percentAttribute);
                    break;

                case PropertyRangeAttribute rangeAttribute:
                    InitializeByRangeAttribute(rangeAttribute);
                    break;

                case PropertyPathAttribute pathAttribute:
                    InitializeByPathAttribute(pathAttribute);
                    break;

                case PropertyStringsAttribute stringsAttribute:
                    InitializeByStringsAttribute(stringsAttribute);
                    break;

                default:
                    InitializeByDefaultAttribute(attribute);
                    break;
            }
        }


        public event EventHandler? ValueChanged;


        public object Source { get; set; }
        public string Path => _info.Name;
        public string Name { get; set; }
        public string? Tips { get; set; }
        public bool IsVisible { get; set; }
        public object? Default { get; set; }
        public bool IsObsolete { get; set; }
        public string? EmptyMessage { get; set; }
        public PropertyMemberElementOptions Options { get; set; }
        public PropertyValue TypeValue { get; set; }


        [MemberNotNull(nameof(_info), nameof(Source), nameof(Name), nameof(Options))]
        private void InitializeCommon(object source, PropertyInfo info, PropertyMemberAttribute attribute, PropertyMemberElementOptions options)
        {
            Source = source;
            Name = options.Name ?? PropertyMemberAttributeExtensions.GetPropertyName(info, attribute) ?? info.Name;
            Tips = PropertyMemberAttributeExtensions.GetPropertyTips(info, attribute);
            IsVisible = attribute == null || attribute.IsVisible;
            EmptyMessage = attribute?.EmptyMessage;
            Options = options;

            this.Default = GetDefaultValue(source, info);
            this.IsObsolete = GetObsoleteAttribute(info) != null;

            _info = info;

            if (source is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _info.Name)
                    {
                        ValueChanged?.Invoke(s, e);
                    }
                };
            }
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByDefaultAttribute(PropertyMemberAttribute attribute)
        {
            var typeValue = CreateDefaultPropertyValue(attribute);
            if (attribute.NoteConverter is null)
            {
                this.TypeValue = typeValue;
            }
            else
            {
                this.TypeValue = new PropertyValue_PropertyValueWithNote(typeValue, attribute.NoteConverter);
            }
        }

        private PropertyValue CreateDefaultPropertyValue(PropertyMemberAttribute attribute)
        {
            if (_info.PropertyType.IsEnum)
            {
                return new PropertyValue_Enum(this, _info.PropertyType);
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return new PropertyValue_Boolean(this);
                case TypeCode.String:
                    if (attribute.IsRegex)
                    {
                        return new PropertyValue_StringRegex(this);
                    }
                    else
                    {
                        return new PropertyValue_String(this);
                    }
                case TypeCode.Int32:
                    return new PropertyValue_Integer(this);
                case TypeCode.Double:
                    if (attribute.HasDecimalPoint)
                    {
                        return new PropertyValue_DoubleFloat(this);
                    }
                    else
                    {
                        return new PropertyValue_Double(this);
                    }
                default:
                    if (_info.PropertyType == typeof(Point))
                    {
                        return new PropertyValue_Point(this);
                    }
                    else if (_info.PropertyType == typeof(Color))
                    {
                        return new PropertyValue_Color(this);
                    }
                    else if (_info.PropertyType == typeof(Size))
                    {
                        return new PropertyValue_Size(this);
                    }
                    else if (_info.PropertyType == typeof(TimeSpan))
                    {
                        return new PropertyValue_TimeSpan(this);
                    }
                    else
                    {
                        return new PropertyValue_Object(this);
                    }
            }
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByRangeAttribute(PropertyRangeAttribute attribute)
        {
            IValueSetter value = attribute.RangeProperty != null ? (IValueSetter)new PropertyValueSource(this.Source, attribute.RangeProperty) : this;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            this.TypeValue = typeCode switch
            {
                TypeCode.Int32 => CreatePropertyValue(new RangeProfile_Integer(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format)),
                TypeCode.Double => CreatePropertyValue(new RangeProfile_Double(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format, attribute.HasDecimalPoint)),
                _ => throw new NotSupportedException(),
            };
        }

        private PropertyValue CreatePropertyValue(RangeProfile_Integer profile)
        {
            if (profile.IsEditable)
            {
                return new PropertyValue_EditableIntegerRange(this, profile);
            }
            else
            {
                return new PropertyValue_IntegerRange(this, profile);
            }
        }

        private PropertyValue CreatePropertyValue(RangeProfile_Double profile)
        {
            if (profile.IsEditable)
            {
                return new PropertyValue_EditableDoubleRange(this, profile);
            }
            else
            {
                return new PropertyValue_DoubleRange(this, profile);
            }
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByPercentAttribute(PropertyPercentAttribute attribute)
        {
            IValueSetter value = attribute.RangeProperty != null ? (IValueSetter)new PropertyValueSource(this.Source, attribute.RangeProperty) : this;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            this.TypeValue = typeCode switch
            {
                TypeCode.Double => new PropertyValue_Percent(this, new RangeProfile_Double(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format, attribute.HasDecimalPoint)),
                _ => throw new NotSupportedException(),
            };
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByPercentFontSizeAttribute(PropertyPercentFontSizeAttribute attribute)
        {
            IValueSetter value = attribute.RangeProperty != null ? (IValueSetter)new PropertyValueSource(this.Source, attribute.RangeProperty) : this;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            if (typeCode != TypeCode.Double) throw new NotSupportedException();

            var rangeProfile = new RangeProfile_Double(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format, attribute.HasDecimalPoint);

            this.TypeValue = attribute.FontType switch
            {
                FontType.Message => new PropertyValue_PercentMessageFontSize(this, rangeProfile),
                FontType.Menu => new PropertyValue_PercentMenuFontSize(this, rangeProfile),
                _ => throw new NotSupportedException(),
            };
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByPathAttribute(PropertyPathAttribute attribute)
        {
            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            this.TypeValue = typeCode switch
            {
                TypeCode.String => new PropertyValue_FilePath(this, attribute.FileDialogType, attribute.Filter, attribute.Note, attribute.DefaultFileName),
                _ => throw new NotSupportedException(),
            };
        }

        [MemberNotNull(nameof(TypeValue))]
        private void InitializeByStringsAttribute(PropertyStringsAttribute attribute)
        {
            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            this.TypeValue = typeCode switch
            {
                TypeCode.String => new PropertyValue_StringMap(this, attribute.Strings),
                _ => throw new NotSupportedException(),
            };
        }


        private static object? GetDefaultValue(object source, PropertyInfo info)
        {
            var attributes = Attribute.GetCustomAttributes(info, typeof(DefaultValueAttribute));
            if (attributes != null && attributes.Length > 0)
            {
                return ((DefaultValueAttribute)attributes[0]).Value;
            }
            else
            {
                return info.GetValue(source); // もとの値
            }
        }

        private static ObsoleteAttribute? GetObsoleteAttribute(PropertyInfo info)
        {
            return (ObsoleteAttribute?)(Attribute.GetCustomAttribute(info, typeof(ObsoleteAttribute)));
        }


        public Type GetValueType()
        {
            return _info.PropertyType;
        }

        public string GetValueString()
        {
            return TypeValue.GetValueString();
        }


        public bool HasCustomValue
        {
            get { return !object.Equals(Default, GetValue()); }
        }

        public void ResetValue()
        {
            SetValue(Default);
        }

        public void InitializeValue()
        {
            var defaultValueAttribute = _info.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute is not null)
            {
                //Debug.WriteLine($"Reset DefaultValue: {Source.GetType().FullName}.{_info.Name} = {defaultValueAttribute.Value}");
                SetValue(defaultValueAttribute.Value);
            }
            else
            {
                var defaultSource = Options?.GetDefaultSource?.Invoke() ?? CreateDefaultSource();
                var value = GetValue(defaultSource);
                //Debug.WriteLine($"Reset DefaultSource: {Source.GetType().FullName}.{_info.Name} = {value}");
                SetValue(value);
            }
        }

        private object CreateDefaultSource()
        {
            var type = this.Source.GetType();
            return Activator.CreateInstance(type) ?? throw new InvalidOperationException("Cannot create instance from object.");
        }

        public void SetValue(object? value)
        {
            _info.SetValue(this.Source, value);
        }

        public void SetValueFromString(string value)
        {
            TypeValue.SetValueFromString(value);
        }

        public object? GetValue()
        {
            return _info.GetValue(this.Source);
        }

        public object? GetValue(object source)
        {
            return _info.GetValue(source);
        }


        public static PropertyMemberElement Create(object source, string name)
        {
            return Create(source, name, PropertyMemberElementOptions.Default);
        }

        /// <summary>
        /// オブジェクトとプロパティ名から PropertyMemberElement を作成する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyMemberElement Create(object source, string name, PropertyMemberElementOptions options)
        {
            var info = source.GetType().GetProperty(name);
            if (info is null) throw new ArgumentException($"{source.GetType()} does not have the property '{name}'");

            var attribute = GetPropertyMemberAttribute(info);
            if (attribute is null) throw new InvalidOperationException($"Need PropertyMemberAttribute at {source.GetType()}.{name}");

            return new PropertyMemberElement(source, info, attribute, options);
        }

        /// <summary>
        /// PropertyMember属性取得
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static PropertyMemberAttribute? GetPropertyMemberAttribute(MemberInfo info)
        {
            return (PropertyMemberAttribute?)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
        }
    }
}
