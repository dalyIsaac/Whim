namespace Whim;

internal record Stack
{
	/// <summary>
	/// The windows in the stack.
	/// </summary>
	public ImmutableList<IWindow> Windows { get; init; } = [];

	/// <summary>
	/// The number of windows in the stack.
	/// </summary>
	public int Count => Windows.Count;

	/// <summary>
	/// The index of the top window in the stack.
	/// </summary>
	public int TopIndex { get; init; }

	/// <summary>
	/// Adds a <paramref name="window"/> to the stack.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The new <see cref="Stack"/> after the add.</returns>
	public Stack AddWindow(IWindow window)
	{
		if (Windows.Contains(window))
		{
			Logger.Debug($"Window {window} already exists in stack");
			return this;
		}

		ImmutableList<IWindow> newWindows = Windows.Add(window);
		return this with { Windows = newWindows, TopIndex = newWindows.Count - 1 };
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
			Logger.Debug($"Window {window} does not exist in stack");
			return this;
		}

		ImmutableList<IWindow> newWindows = Windows.RemoveAt(index);
		int newTopIndex = TopIndex;
		if (index < TopIndex)
		{
			newTopIndex--;
		}
		else if (index == TopIndex)
		{
			newTopIndex = Math.Max(0, TopIndex - 1);
		}

		return this with
		{
			Windows = newWindows,
			TopIndex = newTopIndex,
		};
	}

	/// <summary>
	/// Checks if the stack contains a <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>True if the stack contains the window, false otherwise.</returns>
	public bool Contains(IWindow window) => Windows.Contains(window);

	/// <summary>
	/// Gets the index of a window in the stack.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The index of the window in the stack, or -1 if the window is not in the stack.</returns>
	public int IndexOf(IWindow window) => Windows.IndexOf(window);

	/// <summary>
	/// Gets the first window in the stack.
	/// </summary>
	/// <returns>The first window in the stack, or null if the stack is empty.</returns>
	public IWindow? GetFirstWindow() => Windows.Count == 0 ? null : Windows[0];

	/// <summary>
	/// The window which is currently on top of the stack.
	/// </summary>
	public IWindow Top => Windows[TopIndex];

	/// <summary>
	/// Enumerates over the non-top windows in the stack.
	/// </summary>
	/// <returns>An enumerable of the non-top windows in the stack.</returns>
	public IEnumerable<IWindow> GetNonTopWindows()
	{
		for (int i = 0; i < Windows.Count; i++)
		{
			if (i != TopIndex)
			{
				yield return Windows[i];
			}
		}
	}

	/// <summary>
	/// Swaps two windows in the stack.
	/// </summary>
	/// <param name="index1"></param>
	/// <param name="index2"></param>
	/// <returns></returns>
	public Stack SwapWindows(int index1, int index2)
	{
		if (index1 < 0 || index1 >= Windows.Count)
		{
			Logger.Error($"Index {index1} is out of range");
			return this;
		}

		if (index2 < 0 || index2 >= Windows.Count)
		{
			Logger.Error($"Index {index2} is out of range");
			return this;
		}

		ImmutableList<IWindow> newWindows = Windows.SetItem(index1, Windows[index2]).SetItem(index2, Windows[index1]);
		return this with { Windows = newWindows };
	}

	/// <summary>
	/// Swaps the window at the given index with the window with the next greater index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Stack SwapWithNext(int index) => SwapWindows(index, (index + 1).Mod(Windows.Count));

	/// <summary>
	/// Swaps the window at the given index with the window with the next lesser index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Stack SwapWithPrevious(int index) => SwapWindows(index, (index - 1).Mod(Windows.Count));
}
