namespace Whim.Core
{
    public interface ICommandable
    {
        /// <summary>
        /// Get the commands which this <c>ICommandable</c> can execute.
        /// </summary>
        public ICommand GetCommands();
        public void ExecuteCommand(ICommand command);
    }
}