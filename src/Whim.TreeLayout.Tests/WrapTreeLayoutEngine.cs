using Moq;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This allows us to test SplitFocusedWindow without having to move onto
/// the UI thread.
/// </summary>
internal class WrapTreeLayoutEngine : TreeLayoutEngine
{
	public WrapTreeLayoutEngine(IContext context) : base(context) { }

	internal void SplitFocusedWindowWrapper(IContext context, IWindow? focusedWindow = null)
	{
		Mock<IWindow> windowModel = new();

		PhantomNode phantomNode = new WrapPhantomNode(context, windowModel.Object);

		SplitFocusedWindow(focusedWindow, phantomNode);
	}
}
