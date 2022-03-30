using Windows.Win32.Foundation;

namespace Whim.TreeLayout;

/// <summary>
/// A phantom node represents a phantom window within the layout tree.
/// Unlike a <see cref="WindowNode"/>, a phantom node is specific to the
/// <see cref="TreeLayoutEngine"/> instance.
/// As such phantom nodes have to manage the window itself, instead of relying
/// on the <see cref="IWindowManager"/>.
/// </summary>
public class PhantomNode : LeafNode
{
	private readonly PhantomWindow _phantomWindow;

	private PhantomNode(IWindow windowModel, PhantomWindow phantomWindow, SplitNode? parent = null) : base(windowModel, parent)
	{
		_phantomWindow = phantomWindow;
	}

	public static PhantomNode? CreatePhantomNode(IConfigContext configContext, SplitNode? parent = null)
	{
		PhantomWindow phantomWindow = new();

		IWindow? windowModel = Whim.Window.CreateWindow(phantomWindow.GetHandle(), configContext);

		if (windowModel == null)
		{
			return null;
		}

		return new PhantomNode(windowModel, phantomWindow, parent);
	}

	public void Hide()
	{
		_phantomWindow.Hide();
	}

	public void Close()
	{
		_phantomWindow.Close();
	}
}
