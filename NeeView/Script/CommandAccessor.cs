//#define DEBUG_SHOWMESSAGE

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NeeView
{
    /// <summary>
    /// コマンドアクセス
    /// </summary>
    public class CommandAccessor : ICommandAccessor
    {
        private readonly CommandElement _command;
        private ImmutableDictionary<string, object> _patch = ImmutableDictionary<string, object>.Empty;
        private readonly IAccessDiagnostics _accessDiagnostics;


        public CommandAccessor(CommandElement command, IAccessDiagnostics accessDiagnostics)
        {
            _command = command;
            _accessDiagnostics = accessDiagnostics ?? throw new System.ArgumentNullException(nameof(accessDiagnostics));
            Parameter = _command.Parameter != null ? new PropertyMap($"nv.Command.{_command.Name}.Parameter", _command.Parameter, _accessDiagnostics) : null;
        }

        [WordNodeMember]
        public string Name
        {
            get { return _command.Name; }
        }

        [WordNodeMember]
        public bool IsShowMessage
        {
            get { return _command.IsShowMessage; }
            set { _command.IsShowMessage = value; }
        }

        [WordNodeMember]
        public string ShortCutKey
        {
            get { return _command.ShortCutKey.ToString(); }
            set { _command.ShortCutKey = new ShortcutKey(value); }
        }

        [WordNodeMember]
        public string TouchGesture
        {
            get { return _command.TouchGesture.ToString(); }
            set { _command.TouchGesture = new TouchGesture(value); }
        }

        [WordNodeMember]
        public string MouseGesture
        {
            get { return _command.MouseGesture.ToString(); }
            set { _command.MouseGesture = new MouseSequence(value?.Replace("←", "L", StringComparison.Ordinal).Replace("↑", "U", StringComparison.Ordinal).Replace("→", "R", StringComparison.Ordinal).Replace("↓", "L", StringComparison.Ordinal).Replace("Click", "C", StringComparison.Ordinal) ?? ""); }
        }

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap? Parameter { get; }


        [WordNodeMember]
        public bool Execute(params object[] args)
        {
            var parameter = _command.CreateOverwriteCommandParameter(_patch, _accessDiagnostics);
            var context = new CommandContext(parameter, args, CommandOption.None);
            return AppDispatcher.Invoke(() =>
            {
                if (_command.CanExecute(this, context))
                {
#if DEBUG && DEBUG_SHOWMESSAGE
                    // 通知
                    string message = _command.ExecuteMessage(this, context);
                    if (!string.IsNullOrEmpty(message))
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
                    }
#endif
                    _command.Execute(this, context);
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        [WordNodeMember]
        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            return Clone().AddPatch(patch);
        }


        internal CommandAccessor Clone()
        {
            return (CommandAccessor)this.MemberwiseClone();
        }

        internal CommandAccessor AddPatch(IDictionary<string, object> patch)
        {
            _patch = _patch.AddRange(patch);
            return this;
        }

        internal WordNode CreateWordNode(string commandName)
        {
            var node = WordNodeHelper.CreateClassWordNode(commandName, this.GetType());

            if (Parameter != null)
            {
                node.Children?.Add(Parameter.CreateWordNode(nameof(Parameter)));
            }

            return node;
        }

    }
}
