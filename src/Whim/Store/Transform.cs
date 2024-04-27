using DotNext;

namespace Whim;

/// <summary>
/// Operation describing how to update the state of the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// <see cref="Execute"/> will specify how to update the store.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record Transform<TResult>()
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
public abstract record Transform() : Transform<Empty> { }
