namespace Whim;

// TODO: Order
public interface IWorkspaceManager2
{
	IWorkspace ActiveWorkspace { get; }

	IWorkspaceContainer WorkspaceContainer { get; }

	void Activate(IWorkspace workspace, IMonitor? monitor = null);

	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false);

	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	void MoveWindowToPreviousMonitor(IWindow? window = null);

	void MoveWindowToNextMonitor(IWindow? window = null);

	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null);
}
