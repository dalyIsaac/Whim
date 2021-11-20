using System.Collections.Generic;
using Whim.Core.Commands;

namespace Whim.Core
{
    public interface IWorkspaceManager : IEnumerable<IWorkspace>, ICommandable
    {
        /// <summary>
        /// The active workspace.
        /// </summary>
        public IWorkspace? ActiveWorkspace { get; }

        public void Add(IWorkspace workspaces);

        /// <summary>
        /// Remove the given workspace.
        /// </summary>
        /// <param name="workspace">The workspace to remove.</param>
        public bool Remove(IWorkspace workspace);

        /// <summary>
        /// Try remove a workspace, by the workspace name.
        /// </summary>
        /// <param name="workspaceName">The workspace name to try and remove.</param>
        /// <returns><c>true</c> when the workspace has been removed.</returns>
        public bool Remove(string workspaceName);
    }
}