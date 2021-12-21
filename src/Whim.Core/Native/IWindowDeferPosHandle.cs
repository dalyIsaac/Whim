using System;

namespace Whim.Core;

public interface IWindowDeferPosHandle : IDisposable
{
	void DeferWindowPos(IWindowLocation location);
}
