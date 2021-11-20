using System;
using System.Collections;
using System.Collections.Generic;
using Whim.Core.Commands;

namespace Whim.Core
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private List<IWorkspace> _workspaces = new();
        public Commander Commander { get; } = new();
        public IWorkspace ActiveWorkspace { get; private set; }

        public WorkspaceManager(params IWorkspace[] workspaces)
        {
            _workspaces.AddRange(workspaces);

            if (workspaces.Length == 0)
                throw new Exception("No workspaces provided");

            ActiveWorkspace = workspaces[0];
        }

        public void Add(IWorkspace workspaces)
        {
            _workspaces.Add(workspaces);
        }

        public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(IWorkspace workspace) => _workspaces.Remove(workspace);

        public bool Remove(string workspaceName)
        {
            var workspace = _workspaces.Find(w => w.Name == workspaceName);
            if (workspace == null)
                return false;

            return _workspaces.Remove(workspace);
        }
    }
}