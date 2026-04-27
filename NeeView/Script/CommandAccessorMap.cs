using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public class CommandAccessorMap : IEnumerable<KeyValuePair<string, ICommandAccessor>>
    {
        private readonly Dictionary<string, ICommandAccessor> _map = new();
        private readonly IAccessDiagnostics _accessDiagnostics;
        private readonly CommandTable _commandTable;


        public CommandAccessorMap(CommandTable commandTable, IAccessDiagnostics accessDiagnostics)
        {
            _commandTable = commandTable;
            _accessDiagnostics = accessDiagnostics ?? throw new ArgumentNullException(nameof(accessDiagnostics));

            foreach (var item in commandTable)
            {
                _map.Add(item.Key, new CommandAccessor(item.Value, accessDiagnostics));
            }

            foreach (var item in commandTable.AlternativeCommands)
            {
                Debug.Assert(item.Value.Alternative is not null);
                var command = commandTable[item.Value.Alternative];
                _map.Add(item.Key, new AlternativeCommandAccessor(item.Key, item.Value, command, accessDiagnostics));
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
                var command = GetCommand(key, _commandTable.AlternativeCommands);
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

        internal ICommandAccessor? GetCommand(string key, Dictionary<string, ObsoleteCommandItem> alternatives)
        {
            if (_map.TryGetValue(key, out var command))
            {
                return command;
            }

            // try alternative
            var nameSource = CommandNameSource.Parse(key);
            if (alternatives.TryGetValue(nameSource.Name, out var alternative))
            {
                Debug.Assert(alternative.Alternative is not null);
                var newName = new CommandNameSource(alternative.Alternative, nameSource.Number).FullName;
                if (_map.TryGetValue(newName, out command))
                {
                    return command;
                }
            }

            return null;
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
            if (accessor is AlternativeCommandAccessor alternativeCommand)
            {
                return alternativeCommand.GetObsoleteAttribute();
            }
            else if (accessor is ObsoleteCommandAccessor obsoleteCommand)
            {
                return obsoleteCommand.GetObsoleteAttribute();
            }
            return null;
        }

        internal AlternativeAttribute? GetAlternative(string key)
        {
            var accessor = _map[key];
            if (accessor is AlternativeCommandAccessor alternativeCommand)
            {
                return alternativeCommand.GetAlternativeAttribute();
            }
            else if (accessor is ObsoleteCommandAccessor obsoleteCommand)
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
