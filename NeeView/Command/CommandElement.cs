using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class CommandArgs
    {
        public static CommandArgs Empty { get; } = new CommandArgs(null, CommandOption.None);

        public CommandArgs(object[]? args, CommandOption options)
        {
            this.Args = args ?? CommandElement.EmptyArgs;
            this.Options = options;
        }

        public object[] Args { get; private set; }
        public CommandOption Options { get; private set; }
    }

    public class CommandContext
    {
        public CommandContext(CommandParameter? parameter, object[] args, CommandOption options)
        {
            this.Parameter = parameter;
            this.Args = args ?? CommandElement.EmptyArgs;
            this.Options = options;
        }

        public CommandContext(CommandParameter? parameter, CommandArgs args) : this(parameter, args.Args, args.Options)
        {
        }

        public CommandParameter? Parameter { get; private set; }
        public object[] Args { get; private set; }
        public CommandOption Options { get; private set; }
    }


    public enum CommandGroup
    {
        [AliasName] None,
        [AliasName] Bookmark,
        [AliasName] BookMove,
        [AliasName] BookOrder,
        [AliasName] Effect,
        [AliasName] File,
        [AliasName] FilmStrip,
        [AliasName] ImageScale,
        [AliasName] Move,
        [AliasName] Other,
        [AliasName] Playlist,
        [AliasName] PageOrder,
        [AliasName] PageSetting,
        [AliasName] Panel,
        [AliasName] Script,
        [AliasName] Video,
        [AliasName] ViewManipulation,
        [AliasName] Window,
    }

    public abstract partial class CommandElement : ISearchItem
    {
        public static CommandElement None { get; } = new NoneCommand();

        public static object[] EmptyArgs { get; } = Array.Empty<object>();

        private string? _menuText;
        private string? _remarksText;
        private ShortcutKey _shortCutKey = ShortcutKey.Empty;
        private TouchGesture _touchGesture = TouchGesture.Empty;
        private MouseSequence _mouseGesture = MouseSequence.Empty;
        private bool _isCloneable = true;
        private CommandParameterSource? _parameterSource;
        private CommandElementMemento? _default;


        public CommandElement() : this(null)
        {
        }

        public CommandElement(string? name)
        {
            NameSource = new CommandNameSource(name ?? CommandElementTools.CreateCommandName(this.GetType()), 0);

            bool isObsolete = this.GetType().IsDefined(typeof(ObsoleteAttribute), false);
            var obsoleteString = isObsolete ? $"({TextResources.GetString("Word.Deprecated")}) " : "";

            Text = GetResourceTextRequired(null, null);
            Menu = GetResourceText(nameof(Menu));
            Remarks = obsoleteString + GetResourceText(nameof(Remarks));
        }


        [Subscribable]
        public event EventHandler<ParameterChangedEventArgs>? ParameterChanged;


        private string GetResourceKey(string? property, string? postfix = null)
        {
            var period = (property is null) ? "" : ".";
            return this.GetType().Name + period + property + postfix;
        }

        private string? GetResourceText(string? property, string? postfix = null)
        {
            var resourceKey = GetResourceKey(property, postfix);
            var resourceValue = TextResources.GetString(resourceKey, false);
            return resourceValue;
        }

        private string GetResourceTextRequired(string? property, string? postfix = null)
        {
            var resourceKey = GetResourceKey(property, postfix);
            var resourceValue = TextResources.GetString(resourceKey, true);
            return resourceValue;
        }


        // コマンドの並び優先度
        public int Order { get; set; }

        // コマンド名ソース
        public CommandNameSource NameSource { get; private set; }

        // コマンド名
        public string Name => NameSource.FullName;

        // コマンドのグループ
        public string Group { get; set; } = "";

        // コマンド表示名
        public string Text { get; set; }

        public string LongText => Group + "/" + Text;

        [NotNull]
        public string? Menu
        {
            get { return _menuText ?? Text; }
            set { _menuText = string.IsNullOrEmpty(value) ? null : value; }
        }

        // コマンド説明
        [NotNull]
        public string? Remarks
        {
            get { return _remarksText ?? ""; }
            set { _remarksText = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// 入力情報が変更されたフラグ。
        /// コマンドバインディングの更新判定に使用される。
        /// </summary>
        public bool IsInputGestureDirty { get; set; }

        // ショートカットキー
        public ShortcutKey ShortCutKey
        {
            get { return _shortCutKey; }
            set
            {
                if (_shortCutKey != value)
                {
                    _shortCutKey = value;
                    IsInputGestureDirty = true;
                }
            }
        }

        // タッチ
        public TouchGesture TouchGesture
        {
            get { return _touchGesture; }
            set
            {
                if (_touchGesture != value)
                {
                    _touchGesture = value;
                    IsInputGestureDirty = true;
                }
            }
        }

        // マウスジェスチャー
        public MouseSequence MouseGesture
        {
            get { return _mouseGesture; }
            set
            {
                if (_mouseGesture != value)
                {
                    _mouseGesture = value;
                    IsInputGestureDirty = true;
                }
            }
        }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // クローン可能？
        public bool IsCloneable
        {
            get => _isCloneable && ParameterSource != null;
            set => _isCloneable = value;
        }

        // ペアコマンド
        // TODO: CommandElementを直接指定
        public string? PairPartner { get; set; }


        public CommandParameterSource? ParameterSource
        {
            get { return _parameterSource; }
            set
            {
                if (_parameterSource != value)
                {
                    if (_parameterSource != null)
                    {
                        _parameterSource.ParameterChanged -= ParameterSource_ParameterChanged;
                    }

                    _parameterSource = value;

                    if (_parameterSource != null)
                    {
                        _parameterSource.ParameterChanged += ParameterSource_ParameterChanged;
                    }

                    ParameterChanged?.Invoke(this, new ParameterChangedEventArgs(null));
                }

                void ParameterSource_ParameterChanged(object? sender, ParameterChangedEventArgs e) => ParameterChanged?.Invoke(this, e);
            }
        }


        public CommandParameter? Parameter
        {
            get => ParameterSource?.Get();
            set => ParameterSource?.Set(value);
        }

        public CommandElement? Share { get; private set; }

        public CommandElementMemento DefaultMemento => _default ?? throw new InvalidOperationException("DefaultMemento is not set yet.");


        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(GetSearchText());
                default:
                    throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
            }
        }

        public CommandElement SetShare(CommandElement share)
        {
            Share = share;
            ParameterSource = share.ParameterSource;
            return this;
        }


        // フラグバインディング 
        public virtual Binding? CreateIsCheckedBinding()
        {
            return null;
        }

        // コマンド実行時表示デリゲート
        public virtual string ExecuteMessage(object? sender, CommandContext e)
        {
            return Text;
        }

        public string ExecuteMessage(object? sender, CommandParameter? parameter, CommandArgs args)
        {
            Debug.Assert(parameter?.GetType() == this.Parameter?.GetType());
            return ExecuteMessage(sender, new CommandContext(parameter, args));
        }

        public string ExecuteMessage(object? sender, CommandArgs args)
        {
            return ExecuteMessage(sender, new CommandContext(this.Parameter, args));
        }

        // コマンド実行可能判定
        public virtual bool CanExecute(object? sender, CommandContext e)
        {
            return true;
        }

        public bool CanExecute(object? sender, CommandArgs args)
        {
            return CanExecute(sender, new CommandContext(this.Parameter, args));
        }

        // コマンド実行
        public abstract void Execute(object? sender, CommandContext args);

        public void Execute(object? sender, CommandParameter? parameter, CommandArgs args)
        {
            Debug.Assert(parameter?.GetType() == this.Parameter?.GetType());
            Execute(sender, new CommandContext(parameter, args));
        }

        public void Execute(object? sender, CommandArgs args)
        {
            Execute(sender, new CommandContext(this.Parameter, args));
        }

        // 一時コマンドパラメーター作成
        public CommandParameter? CreateOverwriteCommandParameter(IDictionary<string, object> args, IAccessDiagnostics accessDiagnostics)
        {
            if (this.Parameter == null) return null;

            var clone = (CommandParameter)this.Parameter.Clone();
            if (args == null || !args.Any()) return clone;

            var map = new PropertyMap($"nv.Command.{this.Name}.Parameter", clone, accessDiagnostics);
            foreach (var arg in args)
            {
                map[arg.Key] = arg.Value;
            }

            return clone;
        }

        // 検索用文字列を取得
        public string GetSearchText()
        {
            return string.Join(",", new string[] { this.Group, this.Text, this.Menu, this.Remarks, this.ShortCutKey.GetDisplayString(), this.MouseGesture.ToString(), this.MouseGesture.GetDisplayString(), this.TouchGesture.GetDisplayString() });
        }

        protected virtual CommandElement CloneInstance()
        {
            var type = this.GetType();
            var element = Activator.CreateInstance(type) as CommandElement ?? throw new InvalidOperationException();
            return element;
        }

        // コマンドの複製
        public CommandElement CloneCommand(CommandNameSource name)
        {
            Debug.Assert(_default is not null);

            var clone = CloneInstance();
            clone.RestoreFromCompletedMemento(_default);
            clone.Order = this.Order;
            clone.ClearGestures();

            clone.NameSource = name;

            if (name.Number != 0)
            {
                clone.Text = clone.Text + " " + name.Number.ToString(CultureInfo.InvariantCulture);
                if (clone._menuText != null)
                {
                    clone.Menu = clone.Menu + " " + name.Number.ToString(CultureInfo.InvariantCulture);
                }
            }

            clone.CreateDefaultMemento();

            return clone;
        }


        private void ClearGestures()
        {
            this.ShortCutKey = ShortcutKey.Empty;
            this.TouchGesture = TouchGesture.Empty;
            this.MouseGesture = MouseSequence.Empty;
        }


        public bool IsCloneCommand()
        {
            return NameSource.IsClone;
        }

        public override string? ToString()
        {
            return Name ?? base.ToString();
        }

        public virtual void UpdateDefaultParameter()
        {
        }

        public virtual MenuItem? CreateMenuItem(bool isDefault)
        {
            return null;
        }

        public void CreateDefaultMemento()
        {
            SetDefaultMemento(CreateMemento());
        }

        [Conditional("DEBUG")]
        public void CheckDefaultMemento()
        {
            Debug.Assert(_default is not null);
        }

        public void SetDefaultMemento(CommandElementMemento memento)
        {
#if DEBUG
            Debug.Assert(memento != null);
            Debug.Assert(memento.ShortCutKey != null);
            Debug.Assert(memento.TouchGesture != null);
            Debug.Assert(memento.MouseGesture != null);
            Debug.Assert(memento.IsShowMessage != null);
            if (Share is not null && ParameterSource is not null)
            {
                Debug.Assert(memento.Parameter != null && memento.Parameter.Equals(ParameterSource.GetDefault()));
            }
#endif

            _default = memento;
        }


        #region Memento

        /// <summary>
        /// 記録を取得
        /// </summary>
        /// <param name="trim">既定値のプロパティを省略する</param>
        /// <returns></returns>
        public CommandElementMemento CreateMemento(bool trim = false)
        {

            var memento = new CommandElementMemento();

            memento.ShortCutKey = ShortCutKey;
            memento.TouchGesture = TouchGesture;
            memento.MouseGesture = MouseGesture;
            memento.IsShowMessage = IsShowMessage;
            memento.Parameter = Parameter?.Clone() as CommandParameter;

            Debug.Assert(Parameter == null || JsonCommandParameterConverter.KnownTypes.Contains(Parameter.GetType()));

            if (trim)
            {
                Debug.Assert(_default is not null);

                if (memento.ShortCutKey == _default.ShortCutKey)
                {
                    memento.ShortCutKey = null;
                }
                if (memento.TouchGesture == _default.TouchGesture)
                {
                    memento.TouchGesture = null;
                }
                if (memento.MouseGesture == _default.MouseGesture)
                {
                    memento.MouseGesture = null;
                }
                if (memento.IsShowMessage == _default.IsShowMessage)
                {
                    memento.IsShowMessage = null;
                }
                if (Share is not null || (memento.Parameter is not null && memento.Parameter.Equals(_default.Parameter)))
                {
                    memento.Parameter = null;
                }
            }

            return memento;
        }

        /// <summary>
        /// 復元
        /// </summary>
        /// <param name="memento">復元する記録</param>
        /// <exception cref="InvalidOperationException">既定値が設定されていない</exception>
        public void Restore(CommandElementMemento memento)
        {
            Debug.Assert(_default is not null);

            if (memento == null)
            {
                return;
            }

            ShortCutKey = memento.ShortCutKey ?? _default.ShortCutKey ?? throw new InvalidOperationException();
            TouchGesture = memento.TouchGesture ?? _default.TouchGesture ?? throw new InvalidOperationException();
            MouseGesture = memento.MouseGesture ?? _default.MouseGesture ?? throw new InvalidOperationException();
            IsShowMessage = memento.IsShowMessage ?? _default.IsShowMessage ?? throw new InvalidOperationException();

            // 共有コマンドでない場合、コマンドパラメーターを復元する
            if (Share is null)
            {
                ParameterSource?.Set(memento.Parameter ?? ParameterSource.GetDefault());
            }
        }

        /// <summary>
        /// 完全な記録での復元
        /// </summary>
        /// <param name="memento">省略されていない完全な記録</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">記録が省略されている</exception>
        public void RestoreFromCompletedMemento(CommandElementMemento memento)
        {
            if (memento == null)
            {
                throw new ArgumentNullException(nameof(memento));
            }

            ShortCutKey = memento.ShortCutKey ?? throw new InvalidOperationException();
            TouchGesture = memento.TouchGesture ?? throw new InvalidOperationException();
            MouseGesture = memento.MouseGesture ?? throw new InvalidOperationException();
            IsShowMessage = memento.IsShowMessage ?? throw new InvalidOperationException();

            if (Share is null)
            {
                ParameterSource?.Set(memento.Parameter ?? throw new InvalidOperationException());
            }
        }

        #endregion Memento

        #region GesturesMemento

        public class GesturesMemento
        {
            public ShortcutKey ShortCutKey { get; set; } = ShortcutKey.Empty;
            public TouchGesture TouchGesture { get; set; } = TouchGesture.Empty;
            public MouseSequence MouseGesture { get; set; } = MouseSequence.Empty;

            public bool IsGesturesEquals(GesturesMemento other)
            {
                return ShortCutKey == other.ShortCutKey
                    && TouchGesture == other.TouchGesture
                    && MouseGesture == other.MouseGesture;
            }

            public bool IsEquals(CommandElement other)
            {
                return IsGesturesEquals(other.CreateGesturesMemento());
            }
        }

        public GesturesMemento CreateGesturesMemento()
        {
            var memento = new GesturesMemento();

            memento.ShortCutKey = ShortCutKey;
            memento.TouchGesture = TouchGesture;
            memento.MouseGesture = MouseGesture;

            return memento;
        }

        public void RestoreGestures(GesturesMemento memento)
        {
            if (memento == null) return;

            ShortCutKey = memento.ShortCutKey;
            TouchGesture = memento.TouchGesture;
            MouseGesture = memento.MouseGesture;
        }

        #endregion GesturesMemento
    }


    [Memento]
    public class CommandElementMemento : ICloneable
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ShortcutKey? ShortCutKey { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TouchGesture? TouchGesture { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MouseSequence? MouseGesture { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsShowMessage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommandParameter? Parameter { get; set; }


        public object Clone()
        {
            var clone = (CommandElementMemento)MemberwiseClone();
            clone.Parameter = this.Parameter?.Clone() as CommandParameter;
            return clone;
        }

        public bool IsDefault()
        {
            return this.ShortCutKey == null
                && this.TouchGesture == null
                && this.MouseGesture == null
                && this.IsShowMessage == null
                && this.Parameter == null;
        }

    }
}

