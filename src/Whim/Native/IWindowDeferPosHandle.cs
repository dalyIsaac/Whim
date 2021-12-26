using System;

namespace Whim;

public interface IWindowDeferPosHandle : IDisposable
{
	void DeferWindowPos(IWindowLocation location);
}
