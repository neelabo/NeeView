using NeeLaboratory.Linq;
using NeeView.Data;
using NeeView.Properties;
using NeeView.Susie;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定ウィンドウ項目基底
    /// </summary>
    public class SettingItem
    {
        private SettingItem? _searchResultItem;


        public SettingItem()
        {
            Header = "";
        }

        public SettingItem(string? header)
        {
            this.Header = header ?? "";
        }

        public SettingItem(string? header, string? tips)
        {
            this.Header = header ?? "";
            this.Tips = tips;
        }

        public object? Icon { get; set; }
        public string Header { get; set; }
        public string? Tips { get; set; }
        public IsEnabledPropertyValue? IsEnabled { get; set; }
        public VisibilityPropertyValue? Visibility { get; set; }
        public object? SubContent { get; set; }

        /// <summary>
        /// 検索結果の項目表示用
        /// </summary>
        public SettingItem SearchResultItem
        {
            get => _searchResultItem ?? this;
            set => _searchResultItem = value;
        }


        public UIElement? CreateContent()
        {
            var control = CreateContentInner();
            if (control == null)
            {
                return null;
            }

            this.IsEnabled?.SetBind(control);

            this.Visibility?.SetBind(control);

            return control;
        }

        protected virtual UIElement? CreateContentInner()
        {
            return null;
        }

        public virtual string GetSearchText()
        {
            return Header + " " + Tips;
        }

        public virtual IEnumerable<SettingItem> GetItemCollection()
        {
            yield return this;
        }

        public virtual void InitializeValue()
        {
        }
    }

    public class DataTriggerSource
    {
        public DataTriggerSource(Binding binding, object value, bool isTrue)
        {
            Binging = binding;
            Value = value;
            IsTrue = isTrue;
        }

        public DataTriggerSource(object source, string path, object value, bool isTrue)
        {
            Binging = new Binding(path) { Source = source };
            Value = value;
            IsTrue = isTrue;
        }

        public Binding Binging { get; private set; }
        public object Value { get; private set; }

        /// <summary>
        /// 条件が成立する時に肯定する結果にする
        /// </summary>
        public bool IsTrue { get; private set; }
    }

    /// <summary>
    /// SettingItem を複数まとめたもの
    /// </summary>
    public class SettingItemGroup : SettingItem
    {
        public SettingItemGroup() : base()
        {
        }

        public SettingItemGroup(string header) : base(header)
        {
        }

        public SettingItemGroup(string header, string? tips) : base(header, tips)
        {
        }


        public List<SettingItem> Children { get; private set; } = new List<SettingItem>();

        public DataTriggerSource? IsEnabledTrigger { get; set; }

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel();

            if (IsEnabledTrigger != null)
            {
                var style = new Style(typeof(DockPanel));

                if (IsEnabledTrigger != null)
                {
                    style.Setters.Add(new Setter()
                    {
                        Property = UIElement.IsEnabledProperty,
                        Value = !IsEnabledTrigger.IsTrue,
                    });
                    var dataTrigger = new DataTrigger()
                    {
                        Binding = IsEnabledTrigger.Binging,
                        Value = IsEnabledTrigger.Value,
                    };
                    dataTrigger.Setters.Add(new Setter()
                    {
                        Property = UIElement.IsEnabledProperty,
                        Value = IsEnabledTrigger.IsTrue,
                    });
                    style.Triggers.Add(dataTrigger);
                }

                dockPanel.Style = style;
            }


            foreach (var content in CreateChildContentCollection())
            {
                DockPanel.SetDock(content, Dock.Top);
                dockPanel.Children.Add(content);
            }

            return dockPanel;
        }

        protected IEnumerable<UIElement> CreateChildContentCollection()
        {
            return this.Children != null
                ? this.Children.Select(e => e.CreateContent()).WhereNotNull()
                : Enumerable.Empty<UIElement>();
        }

        public override IEnumerable<SettingItem> GetItemCollection()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var item in child.GetItemCollection())
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                yield return this;
            }
        }

        public override void InitializeValue()
        {
            foreach (var child in Children)
            {
                child.InitializeValue();
            }
        }
    }

    /// <summary>
    /// SettingItem を複数まとめてタイトルをつけたもの
    /// </summary>
    public class SettingItemSection : SettingItemGroup
    {
        public SettingItemSection(string header)
            : base(header)
        {
        }

        public SettingItemSection(string header, string? tips)
            : base(header, tips)
        {
        }

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel()
            {
                Margin = new Thickness(0, 10, 0, 10),
                UseLayoutRounding = true,
            };

            var title = new StackPanel()
            {
                Margin = new Thickness(0, 15, 0, 0),
            };
            {
                var style = new Style(typeof(StackPanel));
                var trigger = new Trigger()
                {
                    Property = UIElement.IsEnabledProperty,
                    Value = false,
                };
                trigger.Setters.Add(new Setter()
                {
                    Property = UIElement.OpacityProperty,
                    Value = 0.5,
                });
                style.Triggers.Add(trigger);
                title.Style = style;
            }

            var titleTextBlock = new TextBlock()
            {
                Text = this.Header,
                FontSize = 24.0,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            title.Children.Add(titleTextBlock);

            if (!string.IsNullOrWhiteSpace(this.Tips))
            {
                var tips = new TextBlock()
                {
                    Text = this.Tips,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0),
                };
                tips.SetResourceReference(TextBlock.ForegroundProperty, "Control.GrayText");
                title.Children.Add(tips);
            }

            DockPanel.SetDock(title, Dock.Top);
            dockPanel.Children.Add(title);

            var subDockPanel = new DockPanel()
            {
                Margin = new Thickness(0, 5, 0, 5),
            };
            foreach (var content in CreateChildContentCollection())
            {
                DockPanel.SetDock(content, Dock.Top);
                subDockPanel.Children.Add(content);
            }
            dockPanel.Children.Add(subDockPanel);

            return dockPanel;
        }
    }

    /// <summary>
    /// PropertyMemberElement を設定項目としたもの
    /// </summary>
    public class SettingItemProperty : SettingItem
    {
        private readonly PropertyMemberElement _element;
        private readonly object? _content;

        public SettingItemProperty(PropertyMemberElement element) : base(element.Name)
        {
            Debug.Assert(element != null);
            _element = element;
            this.Tips = _element.Tips;
        }

        public SettingItemProperty(PropertyMemberElement element, object content) : this(element)
        {
            _content = content;
        }

        public bool IsStretch { get; set; }

        public Action? ResetAction { get; set; }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(this.Icon, this.Header, this.Tips, _content ?? _element.TypeValue, this.SubContent, this.IsStretch);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }

        public override void InitializeValue()
        {
            if (ResetAction is not null)
            {
                ResetAction.Invoke();
            }
            else if (_content is IValueInitializable initializable)
            {
                initializable.InitializeValue();
            }
            else
            {
                _element.InitializeValue();
            }
        }
    }


    public class SettingItemHeader : SettingItem
    {
        public SettingItemHeader(string header) : base(header)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(this.Icon, this.Header, this.Tips, null, this.SubContent, false);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }
    }


    public class SettingItemContent : SettingItem
    {
        private readonly object? _content;

        public SettingItemContent(string header, object content) : base(header)
        {
            _content = content;
        }

        public bool IsStretch { get; set; }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(this.Icon, this.Header, this.Tips, _content, this.SubContent, this.IsStretch);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }
    }


    /// <summary>
    /// 複数の PropertyMemberElement を設定項目としたもの
    /// </summary>
    public class SettingItemMultiProperty : SettingItem
    {
        private readonly PropertyMemberElement _element1;
        private readonly PropertyMemberElement _element2;

        public SettingItemMultiProperty(PropertyMemberElement element1, PropertyMemberElement element2) : base(element1?.ToString())
        {
            Debug.Assert(element1 != null);
            Debug.Assert(element2 != null);
            _element1 = element1;
            _element2 = element2;
        }

        public object? Content1 { get; set; }
        public object? Content2 { get; set; }

        protected override UIElement CreateContentInner()
        {
            var content1 = Content1 ?? _element1.TypeValue;
            var content2 = Content2 ?? _element2.TypeValue;
            return new SettingItemMultiControl(_element1.Name, _element1.Tips ?? this.Tips, content1, content2);
        }

        public override string GetSearchText()
        {
            return _element1.Name + " " + (_element1.Tips ?? this.Tips);
        }

        public override void InitializeValue()
        {
            _element1.InitializeValue();
            _element2.InitializeValue();
        }
    }

    /// <summary>
    /// PropertyMemberElement を補足設定項目としたもの
    /// </summary>
    public class SettingItemSubProperty : SettingItem
    {
        private readonly PropertyMemberElement _element;
        private readonly object? _content;

        public SettingItemSubProperty(PropertyMemberElement element) : base(element.Name)
        {
            Debug.Assert(element != null);
            _element = element;
            this.Tips = element.Tips;
        }

        public SettingItemSubProperty(PropertyMemberElement element, object content) : this(element)
        {
            _content = content;
        }

        public bool IsStretch { get; set; } = true;

        protected override UIElement CreateContentInner()
        {
            return new SettingItemSubControl(this.Header, this.Tips, _content ?? _element.TypeValue, this.IsStretch);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }

        public override void InitializeValue()
        {
            _element.InitializeValue();
        }
    }


    /// <summary>
    /// フォント選択
    /// </summary>
    public class SettingItemPropertyFont : SettingItem
    {
        private readonly PropertyMemberElement _element;

        public SettingItemPropertyFont(PropertyMemberElement element) : base(element?.ToString())
        {
            Debug.Assert(element != null);
            _element = element;
        }

        protected override UIElement CreateContentInner()
        {
            var comboBox = new ComboBox();
            var currentLang = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
            var fonts = Fonts.SystemFontFamilies.Select(v => v.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value ?? v.Source);
            comboBox.ItemsSource = fonts;
            comboBox.IsEditable = false;

            var binding = new Binding(nameof(PropertyValue_String.Value)) { Source = _element.TypeValue as PropertyValue_String };
            BindingOperations.SetBinding(comboBox, ComboBox.SelectedItemProperty, binding);

            return new SettingItemControl(null, _element.Name, _element.Tips ?? this.Tips, comboBox, this.SubContent, false);
        }

        public override string GetSearchText()
        {
            return _element.Name + " " + (_element.Tips ?? this.Tips);
        }

        public override void InitializeValue()
        {
            _element.InitializeValue();
        }
    }


    /// <summary>
    /// IndexValueに対応したプロパティの設定項目
    /// </summary>
    public class SettingItemIndexValue<T> : SettingItem
        where T : struct
    {
        private readonly PropertyMemberElement _element;
        private readonly IndexValue<T> _indexValue;
        private readonly bool _isEditable;

        public SettingItemIndexValue(PropertyMemberElement element, IndexValue<T> indexValue, bool isEditable) : base(element?.ToString())
        {
            Debug.Assert(element != null);

            _element = element;
            _indexValue = indexValue;
            _isEditable = isEditable;

            _indexValue.Property = (PropertyValue<T>)_element.TypeValue;
        }

        protected override UIElement CreateContentInner()
        {
            var content = new SettingItemIndexValueControl()
            {
                IndexValue = _indexValue,
                IsEditable = _isEditable,
            };

            return new SettingItemControl(null, _element.Name, _element.Tips ?? this.Tips, content, this.SubContent, false);
        }

        public override string GetSearchText()
        {
            return _element.Name + " " + (_element.Tips ?? this.Tips);
        }

        public override void InitializeValue()
        {
            _element.InitializeValue();
            _indexValue.Update();
        }
    }


    /// <summary>
    /// ボタンの SettingItem
    /// </summary>
    public class SettingItemButton : SettingItem
    {
        private readonly object? _buttonContent;
        private readonly ICommand _command;

        public bool IsContentOnly { get; set; }

        public SettingItemButton(string header, ICommand command)
            : base(header)
        {
            _command = command;
        }

        public SettingItemButton(string header, object buttonContent, ICommand command)
            : base(header)
        {
            _buttonContent = buttonContent;
            _command = command;
        }



        protected override UIElement CreateContentInner()
        {
            var button = new Button()
            {
                Content = _buttonContent ?? this.Header,
                Command = _command,
                Padding = new Thickness(20, 10, 20, 10),
                MinWidth = 150,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            // 自身をコマンドパラメータとする
            button.CommandParameter = button;

            if (this.IsContentOnly)
            {
                button.Margin = new Thickness(0, 5, 0, 5);
                return button;
            }
            else
            {
                return new SettingItemControl(this.Icon, this.Header, this.Tips, button, this.SubContent, true);
            }
        }
    }


    /// <summary>
    /// リンクの SettingItem
    /// </summary>
    public class SettingItemLink : SettingItem
    {
        private readonly ICommand _command;

        public SettingItemLink(string header, ICommand command)
            : base(header)
        {
            _command = command;
        }

        public bool IsContentOnly { get; set; }

        protected override UIElement CreateContentInner()
        {
            var textBlock = UIElementTools.CreateHyperlink(this.Header, _command);

            if (this.IsContentOnly)
            {
                textBlock.Margin = new Thickness(0, 5, 0, 5);
                return textBlock;
            }
            else
            {
                return new SettingItemControl(this.Icon, this.Header, this.Tips, textBlock, this.SubContent, true);
            }
        }
    }


    /// <summary>
    /// 説明項目
    /// </summary>
    public class SettingItemNote : SettingItem
    {
        private readonly string _text;
        private readonly string _header;

        public SettingItemNote(string text, string header) : base(null)
        {
            _text = text;
            _header = header;
        }

        protected override UIElement CreateContentInner()
        {
            var textBlock = new TextBlock()
            {
                Text = _text,
                Padding = new Thickness(20),
            };

            var groupBox = new GroupBox
            {
                Header = _header,
                Content = textBlock,
                Margin = new Thickness(0, 20, 0, 20),
            };

            return groupBox;
        }

        public override string GetSearchText()
        {
            return "";
        }
    }


    /// <summary>
    /// マウスドラッグのキー設定項目
    /// </summary>
    public class SettingItemMouseDrag : SettingItem
    {
        public SettingItemMouseDrag() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingMouseDragControl();
        }
    }

    /// <summary>
    /// Susieプラグイン設定項目
    /// </summary>
    public class SettingItemSusiePlugin : SettingItem
    {
        private readonly SusiePluginType _pluginType;

        public SettingItemSusiePlugin(SusiePluginType pluginType) : base(null)
        {
            _pluginType = pluginType;
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemSusiePluginControl(_pluginType);
        }
    }

    /// <summary>
    /// ファイル関連づけ設定項目
    /// </summary>
    public class SettingItemFileAssociation : SettingItem
    {
        private  SettingItemFileAssociationControl? _control;

        public SettingItemFileAssociation() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return _control ??= new SettingItemFileAssociationControl();
        }

        public override void InitializeValue()
        {
            _control?.InitializeValue();
        }
    }

    /// <summary>
    /// コマンド設定項目
    /// </summary>
    public class SettingItemCommand : SettingItem
    {
        private SettingItemCommandControl? _control;

        public SettingItemCommand() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            _control = new SettingItemCommandControl();
            return _control;
        }

        public override string GetSearchText()
        {
            return string.Join(" ",
                TextResources.GetString("Word.Command"),
                TextResources.GetString("EditCommandWindow.Tab.Shortcut"),
                TextResources.GetString("EditCommandWindow.Tab.Gesture"),
                TextResources.GetString("EditCommandWindow.Tab.Touch"),
                TextResources.GetString("EditCommandWindow.Tab.Parameter"));
        }

        public override void InitializeValue()
        {
            _control?.Reset();
        }
    }

    /// <summary>
    /// メニュー設定項目
    /// </summary>
    public class SettingItemContextMenu : SettingItem
    {
        private ContextMenuSettingControl? _control;

        public SettingItemContextMenu() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            _control = new ContextMenuSettingControl()
            {
                ContextMenuSetting = ContextMenuSource.Current
            };

            return _control;
        }

        public override void InitializeValue()
        {
            _control?.Reset();
        }
    }



    /// <summary>
    /// UIElement作成補助
    /// </summary>
    public static class UIElementTools
    {
        public static TextBlock CreateHyperlink(string text, ICommand command)
        {
            var textBlock = new TextBlock();
            var link = new Hyperlink();
            link.Inlines.Add(text);
            link.Command = command;
            textBlock.Inlines.Add(link);
            return textBlock;
        }
    }

}
