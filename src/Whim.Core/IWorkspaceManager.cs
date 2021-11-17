namespace Whim.Core
{
    public interface IWorkspaceManager : ICommandable
    {
        /// <summary>
        /// The active workspace.
        /// </summary>
        public IWorkspace ActiveWorkspace { get; }

        /// <summary>
        /// Add the provided workspaces to the workspace manager.
        /// </summary>
        /// <param name="workspaces">The workspaces to manage</param>
        public void AddWorkspaces(params IWorkspace[] workspaces);

        /// <summary>
        /// Remove the given workspace.
        /// </summary>
        /// <param name="workspace">The workspace to remove.</param>
        public void RemoveWorkspace(IWorkspace workspace);

        /// <summary>
        /// Try remove a workspace, by the workspace name.
        /// </summary>
        /// <param name="workspaceName">The workspace name to try and remove.</param>
        /// <returns><c>true</c> when the workspace has been removed.</returns>
        public bool TryRemoveWorkspace(string workspaceName);
    }
}