﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PagemarkConfig Pagemark { get; set; } = new PagemarkConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public LayoutConfig Layout { get; set; } = new LayoutConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();



        /// <summary>
        /// Configプロパティを上書き
        /// </summary>
        public void Merge(Config config)
        {
            if (config == null) throw new ArgumentNullException();
            MergeObject(this, config);
        }

        /// <summary>
        /// インスタンスのプロパティを上書き
        /// TODO: 配列や辞書の対応
        /// </summary>
        private static void MergeObject(object a1, object a2)
        {
            if (a1 == null && a2 == null) return;

            var type = a1.GetType();
            if (type != a2.GetType()) throw new ArgumentException();
            if (!type.IsClass) throw new ArgumentException();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var v1 = property.GetValue(a1);
                var v2 = property.GetValue(a2);

                if (v1 == null && v2 == null)
                {
                }
                else if (property.PropertyType.IsValueType || property.PropertyType == typeof(string) || property.PropertyType.GetCustomAttribute(typeof(PropertyMergeAttribute)) != null)
                {
                    property.GetSetMethod(false)?.Invoke(a1, new object[] { v2 });
                }
                else
                {
                    if (v1 == null)
                    {
                        v1 = Activator.CreateInstance(type);
                        property.SetValue(a1, v1);
                    }
                    MergeObject(v1, v2);
                }
            }
        }

    }

    // この属性がクラスに定義されている場合、リファレンスの変更のみとする
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyMergeAttribute : Attribute
    {
    }
}