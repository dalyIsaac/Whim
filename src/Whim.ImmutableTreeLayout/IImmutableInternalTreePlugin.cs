using System.Collections.Generic;

namespace Whim.ImmutableTreeLayout;

internal interface IImmutableInternalTreePlugin : IPlugin
{
	ISet<IWindow> PhantomWindows { get; }
}
