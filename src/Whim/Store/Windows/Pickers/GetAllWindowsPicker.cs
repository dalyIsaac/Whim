using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Get all the <see cref="IWindow"/>s tracked by Whim.
/// </summary>
public record GetAllWindowsPicker() : Picker<IEnumerable<IWindow>>()
{
	internal override IEnumerable<IWindow> Execute(IContext ctx, IInternalContext internalCtx) =>
		ctx.Store.WindowSlice.Windows.Values;
}
