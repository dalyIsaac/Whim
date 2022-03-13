using System.Windows.Interop;
using Windows.Win32.Foundation;

namespace Whim.TreeLayout;

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

		// We need to show the window in order to get its handle.
		phantomWindow.Show();

		IWindow? windowModel = Whim.Window.CreateWindow((HWND)new WindowInteropHelper(phantomWindow).Handle, configContext);

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
