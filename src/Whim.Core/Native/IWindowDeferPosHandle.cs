using System;

namespace Whim.Core.Native;

public interface IWindowDeferPosHandle : IDisposable
{
	void DeferWindowPos(IWindowLocation location);
}
