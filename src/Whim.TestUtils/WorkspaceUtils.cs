using NSubstitute;

namespace Whim.TestUtils;

public static class WorkspaceUtils
{
	public static IWorkspace[] CreateWorkspaces(int count)
	{
		IWorkspace[] workspaces = new IWorkspace[count];
		for (int i = 0; i < count; i++)
		{
			workspaces[i] = Substitute.For<IWorkspace, IInternalWorkspace>();
		}
		return workspaces;
	}
}
