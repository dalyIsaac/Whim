namespace Whim;

// This has to be public in order for IButler to reexport it.
public interface IButlerChores
{
	void Activate(IWorkspace workspace, IMonitor? monitor = null);

	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	void LayoutAllActiveWorkspaces();

	bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null);

	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	void MoveWindowToNextMonitor(IWindow? window = null);

	void MoveWindowToPreviousMonitor(IWindow? window = null);

	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false);
}
