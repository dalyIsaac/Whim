using System.Collections;
using System.Collections.Generic;

namespace Whim.Core.Command
{
    using CommanderValues = KeyValuePair<CommandType, CommandHandler>;
    public delegate void CommandHandler(ICommand command);

    public class Commander : IEnumerable<CommanderValues>
    {
        private readonly Dictionary<CommandType, CommandHandler> _ownerCommand = new();
        private readonly List<Commander> _children = new();

        public IEnumerator<CommanderValues> GetEnumerator() => _ownerCommand.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(CommandType commandType, CommandHandler commandHandler)
        {
            if (_ownerCommand.ContainsKey(commandType))
            {
                throw new System.Exception($"Command {commandType} already exists");
            }

            _ownerCommand.Add(commandType, commandHandler);
        }

        public void Add(params Commander[] childCommanders) => _children.AddRange(childCommanders);

        public void ExecuteCommand(ICommand command)
        {
            if (_ownerCommand.TryGetValue(command.Type, out var commandHandler))
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