using NeeView.Properties;
using NeeView.Text;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace NeeView
{
    public class ScriptCommand : CommandElement, IDisposable
    {
        private class ScriptCommandParameterDecorator : ICommandParameterDecorator
        {
            private readonly string _path;
            private readonly ScriptCommandSourceMap _sourceMap;

            public ScriptCommandParameterDecorator(string path, ScriptCommandSourceMap sourceMap)
            {
                _path = path;
                _sourceMap = sourceMap;
            }

            public void DecorateCommandParameter(CommandParameter parameter)
            {
                if (parameter is not ScriptCommandParameter scriptCommandParameter) return;

                if (_sourceMap.TryGetValue(_path, out var source))
                {
                    scriptCommandParameter.Argument = source.Args;
                }
            }
        }


        public const string Prefix = "Script_";
        public const string EventOnStartup = Prefix + ScriptCommandSource.OnStartupFilename;
        public const string EventOnBookLoaded = Prefix + ScriptCommandSource.OnBookLoadedFilename;
        public const string EventOnPageChanged = Prefix + ScriptCommandSource.OnPageChangedFilename;
        public const string EventOnPageEnd = Prefix + ScriptCommandSource.OnPageEndFilename;
        public const string EventOnWindowStateChanged = Prefix + ScriptCommandSource.OnWindowStateChangedFilename;

        private readonly string _path;
        private readonly ScriptCommandSourceMap _sourceMap;
        private GesturesMemento? _defaultGestures;
        private string? _defaultArgs;
        private ScriptIsCheckedBindingSource? _bindingSource;
        private bool _disposedValue;

        public ScriptCommand(string path, ScriptCommandSourceMap sourceMap) : base(PathToScriptCommandName(path))
        {
            _path = path;
            _sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));

            this.Group = TextResources.GetString("CommandGroup.Script");
            this.Text = LoosePath.GetFileNameWithoutExtension(_path);

            this.ParameterSource = new CommandParameterSource(new ScriptCommandParameter(), new ScriptCommandParameterDecorator(path, sourceMap));

            UpdateDocument(true);
        }


        public string Path => _path;


        public static bool IsScriptCommandName(string name)
        {
            return name.StartsWith(Prefix, StringComparison.Ordinal);
        }

        public static string PathToScriptCommandName(string path)
        {
            return Prefix + LoosePath.GetFileNameWithoutExtension(path);
        }

        protected override CommandElement CloneInstance()
        {
            var command = new ScriptCommand(_path, _sourceMap);
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                command.Text = source.Text;
                command.Remarks = source.Remarks;
                command.ShortCutKey = ShortcutKey.Empty;
                command.TouchGesture = TouchGesture.Empty;
                command.MouseGesture = MouseSequence.Empty;
            }
            return command;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var args = e.Args.Length > 0 ? e.Args.ToList() : StringTools.SplitArgument((e.Parameter.Cast<ScriptCommandParameter>()).Argument).ToList<object>();
            ScriptManager.Current.Execute(sender, _path, Name, args);
        }

        public override void UpdateDefaultParameter()
        {
            StoreDefaultArgs();
        }

        private void StoreDefaultGesture()
        {
            _defaultGestures = CreateGesturesMemento();
        }

        private void StoreDefaultArgs()
        {
            _defaultArgs = GetDefaultArgs();
        }

        private string? GetDefaultArgs()
        {
            return (ParameterSource?.GetDefault() as ScriptCommandParameter)?.Argument;
        }

        public ScriptCommandParameter GetScriptCommandParameter()
        {
            return (Parameter as ScriptCommandParameter) ?? throw new InvalidOperationException();
        }

        public void UpdateDocument(bool isForce)
        {
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                IsCloneable = source.IsCloneable;

                Text = source.Text;
                if (IsCloneCommand())
                {
                    Text += " " + NameSource.Number.ToString(CultureInfo.InvariantCulture);
                }

                Remarks = source.Remarks;

                if (isForce || (_defaultGestures != null && _defaultGestures.IsEquals(this) && !IsCloneCommand()))
                {
                    ShortCutKey = new ShortcutKey(source.ShortCutKey);
                    MouseGesture = new MouseSequence(source.MouseGesture);
                    TouchGesture = new TouchGesture(source.TouchGesture);
                    StoreDefaultGesture();
                }

                var parameter = GetScriptCommandParameter();
                if (isForce || parameter.Argument == _defaultArgs)
                {
                    parameter.Argument = source.Args;
                    StoreDefaultArgs();
                }
            }
        }

        public void OpenFile()
        {
            ExternalProcess.OpenWithTextEditor(_path);
        }


        public override Binding? CreateIsCheckedBinding()
        {
            if (_disposedValue) return null;

            _bindingSource ??= new ScriptIsCheckedBindingSource(this);
            return new Binding(nameof(_bindingSource.IsChecked)) { Source = _bindingSource, Mode = BindingMode.OneWay };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _bindingSource?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
