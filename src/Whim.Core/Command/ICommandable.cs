namespace Whim.Core.Command
{
    public interface ICommandable
    {
        public Commander Commander { get; }
    }
}