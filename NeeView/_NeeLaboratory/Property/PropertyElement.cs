﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeLaboratory.Property
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

    /// <summary>
    /// プロパティ項目表示編集
    /// </summary>
    public class PropertyMemberElement : PropertyDrawElement, IValueSetter
    {
        public object Source { get; set; }
        public string Name { get; set; }
        public string Tips { get; set; }
        public object Default { get; set; }

        private PropertyInfo _info;

        public PropertyMemberElement(object source, PropertyInfo info, PropertyMemberAttribute attribute)
        {
            Source = source;
            Name = attribute.Name ?? info.Name;
            Tips = attribute.Tips;
            Default = GetDefaultValue(source, info);

            _info = info;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    this.TypeValue = new PropertyValue_Boolean(this);
                    break;
                case TypeCode.String:
                    this.TypeValue = new PropertyValue_String(this);
                    break;
                case TypeCode.Int32:
                    this.TypeValue = new PropertyValue_Integer(this);
                    break;
                case TypeCode.Double:
                    this.TypeValue = new PropertyValue_Double(this);
                    break;
                default:
                    if (_info.PropertyType == typeof(Point))
                    {
                        this.TypeValue = new PropertyValue_Point(this);
                    }
                    else if (_info.PropertyType == typeof(Color))
                    {
                        this.TypeValue = new PropertyValue_Color(this);
                    }
                    else
                    {
                        this.TypeValue = new PropertyValue_Object(this);
                    }
                    break;
            }
        }

        //
        private static object GetDefaultValue(object source, PropertyInfo info)
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


        //
        public void ResetValue()
        {
            SetValue(Default);
        }

        //
        public void SetValue(object value)
        {
            this._info.SetValue(this.Source, value);
        }

        //
        public object GetValue()
        {
            return this._info.GetValue(this.Source);
        }

        //
        public object GetValue(object source)
        {
            return this._info.GetValue(source);
        }

        //
        public PropertyValue TypeValue { get; set; }
    }


    //
    public class PropertyRangeElement : PropertyMemberElement
    {
        public PropertyRangeAttribute Range { get; private set; }

        public PropertyRangeElement(object source, PropertyInfo info, PropertyRangeAttribute attribute) : base(source, info, attribute)
        {
            Range = attribute;
        }
    }


}
