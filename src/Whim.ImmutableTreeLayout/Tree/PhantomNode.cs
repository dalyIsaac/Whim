namespace Whim.ImmutableTreeLayout;

/// <summary>
/// A phantom node represents a phantom window within the layout tree.
/// Unlike a <see cref="WindowNode"/>, a phantom node is specific to the
/// layout engine.
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
	protected PhantomNode(IContext context, IWindow windowModel, Microsoft.UI.Xaml.Window? phantomWindow)
		: base(windowModel)
	{
		_context = context;
		_phantomWindow = phantomWindow;
	}

	/// <summary>
	/// Creates a new phantom window. If the window model could not be retrieved, <see langword="null"/> is returned.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static PhantomNode? CreatePhantomNode(IContext context)
	{
		PhantomWindow phantomWindow = new(context);

		IWindow? windowModel = context.WindowManager.CreateWindow(phantomWindow.GetHandle());

		if (windowModel == null)
		{
			return null;
		}

		return new PhantomNode(context, windowModel, phantomWindow);
	}

	/// <inheritdoc/>
	public void Hide() => _phantomWindow?.Hide(_context);

	/// <inheritdoc/>
	public void Close() => _phantomWindow?.Close();
}
