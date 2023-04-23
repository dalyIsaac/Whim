namespace Whim.TreeLayout;

/// <summary>
/// A phantom node represents a phantom window within the layout tree.
/// Unlike a <see cref="WindowNode"/>, a phantom node is specific to the
/// <see cref="ITreeLayoutEngine"/> instance.
/// As such phantom nodes have to manage the window itself, instead of relying
/// on the <see cref="IWindowManager"/>.
/// </summary>
internal class PhantomNode : LeafNode
{
	private readonly IContext _context;
	private readonly Microsoft.UI.Xaml.Window? _phantomWindow;

	/// <summary>
	/// Creates a new phantom node.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="windowModel"></param>
	/// <param name="phantomWindow"></param>
	/// <param name="parent"></param>
	protected PhantomNode(
		IContext context,
		IWindow windowModel,
		Microsoft.UI.Xaml.Window? phantomWindow,
		SplitNode? parent = null
	)
		: base(windowModel, parent)
	{
		_context = context;
		_phantomWindow = phantomWindow;
	}

	/// <summary>
	/// Creates a new phantom window. If the window model could not be retrieved, <see langword="null"/> is returned.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static PhantomNode? CreatePhantomNode(IContext context, SplitNode? parent = null)
	{
		PhantomWindow phantomWindow = new(context);

		IWindow? windowModel = context.WindowManager.CreateWindow(phantomWindow.GetHandle());

		if (windowModel == null)
		{
			return null;
		}

		return new PhantomNode(context, windowModel, phantomWindow, parent);
	}

	/// <inheritdoc/>
	public void Hide() => _phantomWindow?.Hide(_context);

	/// <inheritdoc/>
	public void Close() => _phantomWindow?.Close();
}
