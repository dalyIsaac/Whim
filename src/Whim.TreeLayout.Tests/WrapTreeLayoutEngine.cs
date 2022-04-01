using Moq;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This allows us to test SplitFocusedWindow without having to move onto
/// the UI thread.
/// </summary>
internal class WrapTreeLayoutEngine : TreeLayoutEngine
{
	public WrapTreeLayoutEngine(IConfigContext configContext) : base(configContext)
	{
	}

	internal void SplitFocusedWindowWrapper(IWindow? focusedWindow = null)
	{
		Mock<IWindow> windowModel = new();

		PhantomNode phantomNode = new WrapPhantomNode(windowModel.Object);

		SplitFocusedWindow(focusedWindow, phantomNode);
	}
}
