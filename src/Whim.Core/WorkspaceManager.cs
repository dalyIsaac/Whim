using System;
using System.Collections;
using System.Collections.Generic;
using Whim.Core.Commands;

namespace Whim.Core
{
    public class WorkspaceManager : IWorkspaceManager
    {
        public Commander Commander { get; } = new();
        private List<IWorkspace> _workspaces = new();
        public IWorkspace? ActiveWorkspace { get; private set; }

        public WorkspaceManager(params IWorkspace[] workspaces)
        {
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