namespace Whim;

/// <summary>
/// Description of how to retrieve data from the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult">The type of the resulting data from the store.</typeparam>
public abstract record Picker<TResult>()
{
	/// <summary>
	/// How to fetch the data from the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <returns></returns>
	internal abstract TResult Execute(IContext ctx, IInternalContext internalCtx);
}

/// <summary>
/// Description of how to retrieve data from the <see cref="Store"/>.
/// Pure pickers can be implemented in terms of pure functions.
/// </summary>
/// <typeparam name="TResult"></typeparam>
/// <param name="rootSector"></param>
/// <returns></returns>
public delegate TResult PurePicker<TResult>(IRootSector rootSector);
