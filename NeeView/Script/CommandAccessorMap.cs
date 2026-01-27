using System;
using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    public class CommandAccessorMap : IEnumerable<KeyValuePair<string, ICommandAccessor>>
    {
        private readonly Dictionary<string, ICommandAccessor> _map = new();
        private readonly IAccessDiagnostics _accessDiagnostics;


        public CommandAccessorMap(CommandTable commandTable, IAccessDiagnostics accessDiagnostics)
        {
            _accessDiagnostics = accessDiagnostics ?? throw new ArgumentNullException(nameof(accessDiagnostics));

            foreach (var item in commandTable)
            {
                _map.Add(item.Key, new CommandAccessor(item.Value, accessDiagnostics));
            }

            foreach (var item in commandTable.ObsoleteCommands)
            {
                _map.Add(item.Key, new ObsoleteCommandAccessor(item.Key, item.Value, accessDiagnostics));
            }
        }


        public ICommandAccessor? this[string key]
        {
            get
            {
                var command = GetCommand(key);
                if (command is ObsoleteCommandAccessor obsoleteCommand)
                {
                    return _accessDiagnostics.Throw<ICommandAccessor>(new NotSupportedException(obsoleteCommand.CreateObsoleteCommandMessage()));
                }
                return command;
            }
        }

        internal bool TryGetCommand(string key, out ICommandAccessor? command)
        {
            return _map.TryGetValue(key, out command);
        }

        internal ICommandAccessor GetCommand(string key)
        {
            return _map[key];
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();
            foreach (var commandName in _map.Keys)
            {
                if (_map[commandName] is CommandAccessor commandAccessor)
                {
                    node.Children.Add(commandAccessor.CreateWordNode(commandName));
                }
            }
            return node;
        }

        internal ObsoleteAttribute? GetObsolete(string key)
        {
            var accessor = _map[key];
            if (accessor is ObsoleteCommandAccessor obsoleteCommand)
            {
                return obsoleteCommand.GetObsoleteAttribute();
            }
            return null;
        }

        internal AlternativeAttribute? GetAlternative(string key)
        {
            var accessor = _map[key];
            if (accessor is ObsoleteCommandAccessor obsoleteCommand)
            {
                return obsoleteCommand.GetAlternativeAttribute();
            }
            return null;
        }

        public IEnumerator<KeyValuePair<string, ICommandAccessor>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
