namespace Whim;

internal record Stack(ImmutableList<IWindow> Windows)
{
	/// <summary>
	/// The index of the window which is currently on top of the stack.
	/// </summary>
	private int _topIndex;

	/// <summary>
	/// The window which is currently on top of the stack.
	/// </summary>
	public IWindow Top => Windows[_topIndex];

	/// <summary>
	/// The number of windows in the stack.
	/// </summary>
	public int Count => Windows.Count;

	/// <summary>
	/// Adds a <paramref name="window"/> to the stack.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The new <see cref="Stack"/> after the add.</returns>
	public Stack AddWindow(IWindow window)
	{
		ImmutableList<IWindow> newWindows = Windows.Add(window);
		return new Stack(newWindows) with { _topIndex = newWindows.Count - 1 };
	}

	/// <summary>
	/// Removes a <paramref name="window"/> from the stack.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The new <see cref="Stack"/> after the remove.</returns>
	public Stack RemoveWindow(IWindow window)
	{
		int index = Windows.IndexOf(window);
		if (index == -1)
		{
			return this;
		}

		ImmutableList<IWindow> newWindows = Windows.RemoveAt(index);
		int newTopIndex = _topIndex;
		if (index < _topIndex)
		{
			newTopIndex--;
		}
		else if (index == _topIndex)
		{
			newTopIndex = Math.Max(0, _topIndex - 1);
		}

		return new Stack(newWindows) with
		{
			_topIndex = newTopIndex,
		};
	}
}
