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
	private readonly Microsoft.UI.Xaml.Window? _phantomWindow;

	/// <summary>
	/// Creates a new phantom node.
	/// </summary>
	/// <param name="windowModel"></param>
	/// <param name="phantomWindow"></param>
	/// <param name="parent"></param>
	protected PhantomNode(IWindow windowModel, Microsoft.UI.Xaml.Window? phantomWindow, SplitNode? parent = null) : base(windowModel, parent)
	{
		_phantomWindow = phantomWindow;
	}

	/// <summary>
	/// Creates a new phantom window. If the window model could not be retrieved, <see langword="null"/> is returned.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static PhantomNode? CreatePhantomNode(IConfigContext configContext, SplitNode? parent = null)
	{
		PhantomWindow phantomWindow = new();

		IWindow? windowModel = Whim.IWindow.CreateWindow(phantomWindow.GetHandle(), configContext);

		if (windowModel == null)
		{
			return null;
		}

		return new PhantomNode(windowModel, phantomWindow, parent);
	}

	/// <inheritdoc/>
	public void Hide() => _phantomWindow?.Hide();

	/// <inheritdoc/>
	public void Close() => _phantomWindow?.Close();
}
