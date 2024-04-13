using System.Collections.Immutable;

namespace Whim;

public class WorkspacesSlice
{
	public ImmutableList<IWorkspace> Workspaces { get; } = ImmutableList<IWorkspace>.Empty;
	public int ActiveWorkspaceIndex { get; } = -1;
}
