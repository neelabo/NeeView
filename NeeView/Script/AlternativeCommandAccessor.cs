using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 廃止されたコマンドのアクセサ。そのまま代替コマンドとして実行する。
    /// </summary>
    public class AlternativeCommandAccessor : ICommandAccessor
    {
        private readonly string _name;
        private readonly ObsoleteCommandItem _obsoleteInfo;
        private readonly CommandAccessor _commandAccessor;

        public AlternativeCommandAccessor(string commandName, ObsoleteCommandItem obsoleteInfo, CommandElement command, IAccessDiagnostics accessDiagnostics)
        {
            _name = commandName;
            _obsoleteInfo = obsoleteInfo;
            _commandAccessor = new CommandAccessor(command, accessDiagnostics);
        }

        public string Name => _commandAccessor.Name;

        public bool IsShowMessage
        {
            get => _commandAccessor.IsShowMessage;
            set => _commandAccessor.IsShowMessage = value;
        }

        public string MouseGesture
        {
            get => _commandAccessor.MouseGesture;
            set => _commandAccessor.MouseGesture = value;
        }

        public PropertyMap? Parameter => _commandAccessor.Parameter;

        public string ShortCutKey
        {
            get => _commandAccessor.ShortCutKey;
            set => _commandAccessor.ShortCutKey = value;
        }

        public string TouchGesture
        {
            get => _commandAccessor.TouchGesture;
            set => _commandAccessor.TouchGesture = value;
        }

        public bool Execute(params object[] args)
        {
            return _commandAccessor.Execute(args);
        }

        public CommandAccessor? Patch(IDictionary<string, object> patch)
        {
            return _commandAccessor.Patch(patch);
        }
        internal ObsoleteAttribute GetObsoleteAttribute()
        {
            return new ObsoleteAttribute();
        }

        internal AlternativeAttribute? GetAlternativeAttribute()
        {
            return _obsoleteInfo != null ? new AlternativeAttribute(_obsoleteInfo.Alternative, _obsoleteInfo.Version, ScriptErrorLevel.Info) : null;
        }

        internal string? CreateObsoleteCommandMessage()
        {
            return RefrectionTools.CreateObsoleteMessage(_name, GetObsoleteAttribute(), GetAlternativeAttribute());
        }
    }

}
