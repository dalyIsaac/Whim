namespace Whim.Core.Commands
{
    public interface ICommand {
        public CommandType Type { get; }
    }

    public interface ICommand<CommandPayloadType> : ICommand
    {
        public CommandPayloadType? Payload { get; }
    }
}