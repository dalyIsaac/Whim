namespace Whim.TreeLayout.Tests;

internal class WrapPhantomNode : PhantomNode
{
	public WrapPhantomNode(IConfigContext configContext, IWindow windowModel, SplitNode? parent = null)
		: base(configContext, windowModel, null, parent) { }
}
