namespace Whim.Core.Commands
{
    public interface ICommandable
    {
        public Commander Commander { get; }
    }
}