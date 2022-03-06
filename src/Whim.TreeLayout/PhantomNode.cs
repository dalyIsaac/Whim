namespace Whim.TreeLayout;

public class PhantomNode : LeafNode
{
	private readonly PhantomWindow _window;

	public PhantomNode(SplitNode? parent = null)
	{
		Parent = parent;
		_window = new PhantomWindow();
		Show();
	}

	public void Show()
	{
		// TODO: get location from parent
		_window.Show();
	}

	public void Hide()
	{
		_window.Hide();
	}

	public void Close()
	{
		_window.Close();
	}

	public override void Focus()
	{
		_window.Focus();
	}
}
