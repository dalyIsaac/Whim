namespace Whim;

/// <summary>
/// Operation describing how to update the state of the <see cref="IStore"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record Transform<TResult>
{
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <param name="rootSector">The root sector.</param>
	internal abstract Result<TResult> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector);
}

/// <inheritdoc/>
public abstract record Transform : WhimTransform<Unit> { }

/// <summary>
/// Operation describing how to update the state of the <see cref="IStore"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record WhimTransform<TResult>
{
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <param name="rootSector">The root sector.</param>
	internal abstract WhimResult<TResult> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	);
}

/// <inheritdoc/>
public abstract record WhimTransform : WhimTransform<Unit> { }
