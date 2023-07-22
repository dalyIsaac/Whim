using System;

namespace Whim;

/// <summary>
/// The unique identity of a layout engine within a <see cref="IWorkspace"/>.
/// </summary>
/// <remarks>
/// This is used to determine if a <see cref="ILayoutEngine"/>  is the same as another
/// layout engine.
/// The leaf layout engines will have an identity, and <see cref="BaseProxyLayoutEngine"/>
/// will proxy the identity of the inner layout engine.
/// Creating a new layout engine from an operation on an existing layout engine will keep the same
/// identity.
/// </remarks>
public sealed class LayoutEngineIdentity
{
	private readonly Guid _id;

	internal LayoutEngineIdentity()
	{
		_id = Guid.NewGuid();
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is LayoutEngineIdentity identity && _id == identity._id;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_id);
}
