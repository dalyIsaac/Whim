namespace Whim;

public abstract record Transform()
{
	public abstract void Execute(IRootSlice root);
}

public interface IRootSlice
{
	MonitorsSlice MonitorsSlice { get; }
	WorkspacesSlice WorkspacesSlice { get; }
	MapsSlice MapsSlice { get; }
	WindowsSlice WindowsSlice { get; }
}

internal class Store : IRootSlice
{
	public MonitorsSlice MonitorsSlice { get; } = new();
	public WorkspacesSlice WorkspacesSlice { get; } = new();
	public MapsSlice MapsSlice { get; } = new();
	public WindowsSlice WindowsSlice { get; } = new();

	public void Dispatch(Transform transform)
	{
		// TODO: reader-writer lock
		transform.Execute(this);
	}
}
