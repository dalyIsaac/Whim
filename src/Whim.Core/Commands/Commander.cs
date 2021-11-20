// register types
// register types for children
using System.Collections;
using System.Collections.Generic;

namespace Whim.Core.Commands
{
    using CommanderValues = KeyValuePair<CommandType, CommandHandler>;
    public delegate void CommandHandler(ICommand command);

    public class Commander : IEnumerable<CommanderValues>
    {
        private readonly Dictionary<CommandType, CommandHandler> _ownerCommands = new();
        private readonly List<Commander> _children = new();

        public IEnumerator<CommanderValues> GetEnumerator() => _ownerCommands.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(CommandType commandType, CommandHandler commandHandler)
        {
            if (_ownerCommands.ContainsKey(commandType))
            {
                throw new System.Exception($"Command {commandType} already exists");
            }

            _ownerCommands.Add(commandType, commandHandler);
        }

        public void Add(params Commander[] childCommanders) => _children.AddRange(childCommanders);

        public void ExecuteCommand(ICommand command)
        {
            if (_ownerCommands.TryGetValue(command.Type, out var commandHandler))
            {
                commandHandler(command);
            }
            else
            {
                foreach (var childCommander in _children)
                {
                    childCommander.ExecuteCommand(command);
                }
            }
        }
    }
}