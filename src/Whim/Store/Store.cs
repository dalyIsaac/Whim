namespace Whim;

/// <summary>
/// Operation describing how to update the <see cref="RootSlice"/> for the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// <see cref="Execute"/> will specify how to update the store.
/// </summary>
public abstract record Transform()
{
	// TODO: How does this interact with internal members?
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="root">The root slice.</param>
	internal abstract void Execute(RootSlice root);
}

/// <summary>
/// Description of how to retrieve data from the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult">The type of the resulting data from the store.</typeparam>
public abstract record Pick<TResult>()
{
	/// <summary>
	/// How to fetch the data from the store.
	/// </summary>
	/// <param name="root">The root slice.</param>
	/// <returns></returns>
	internal abstract TResult Execute(RootSlice root);
}

/// <summary>
/// The root slice for the <see cref="Store"/>.
/// </summary>
internal class RootSlice
{
	public MonitorSlice MonitorSlice { get; } = new MonitorSlice();
	public WorkspaceSlice WorkspaceSlice { get; } = new WorkspaceSlice();
	public MapSlice MapSlice { get; } = new MapSlice();
	public WindowSlice WindowSlice { get; } = new WindowSlice();
}

/// <summary>
/// Whim's store.
/// </summary>
public class Store
{
	private readonly RootSlice _rootSlice = new();

	/// <summary>
	/// Entry point for updates to transform Whim's state.
	/// </summary>
	/// <param name="transform">
	/// The record implementing <see cref="Transform"/> to update Whim's state.
	/// </param>
	public void Transform(Transform transform)
	{
		// TODO: reader-writer lock
		transform.Execute(_rootSlice);
	}

	/// <summary>
	/// Entry-point to pick from Whim's state.
	/// </summary>
	/// <typeparam name="TResult">
	/// The type of the resulting data from the store.
	/// </typeparam>
	/// <param name="pick">
	/// The record implementing <see cref="Pick"/> to fetch from Whim's state.
	/// </param>
	/// <returns></returns>
	public TResult Pick<TResult>(Pick<TResult> pick)
	{
		// TODO: reader-writer lock
		return pick.Execute(_rootSlice);
	}
}
