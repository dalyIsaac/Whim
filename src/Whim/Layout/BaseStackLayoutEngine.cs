using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Abstract layout engine with a stack data structure.
/// Stack layout engines should only implement <see cref="WindowDirection.Left"/>
/// and <see cref="WindowDirection.Right"/>.
/// </summary>
public abstract class BaseStackLayoutEngine : ILayoutEngine
{
	#region Primary area fields
	protected readonly int _primaryAreaBaseCount = 1;
	protected int _primaryAreaCountOffset;
	protected int _primaryAreaTotal => _primaryAreaBaseCount + _primaryAreaCountOffset;

	protected readonly int _primaryAreaSizePercent;
	protected int _primaryAreaSizePercentOffset = 0;
	protected readonly int _primaryAreaSizePercentIncrement;
	#endregion

	protected IConfigContext _configContext;

	protected readonly List<IWindow> _stack = new();

	public Commander Commander { get; } = new();

	public bool LeftToRight { get; }

	public string Name { get; }

	public int Count => _stack.Count;

	public bool IsReadOnly => false;

	public BaseStackLayoutEngine(IConfigContext configContext, string name, bool leftToRight, int primaryPercentBase, int primaryPercentIncrement = 5)
	{
		_configContext = configContext;
		Name = name;
		LeftToRight = leftToRight;
		_primaryAreaSizePercent = primaryPercentBase;
		_primaryAreaSizePercentIncrement = primaryPercentIncrement;
	}

	public void Add(IWindow window)
	{
		Logger.Debug($"Adding window {window.Title} to layout engine {Name}");
		_stack.Add(window);
	}

	public bool Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window.Title} from layout engine {Name}");
		return _stack.RemoveAll(x => x.Handle == window.Handle) > 0;
	}

	public void Clear()
	{
		Logger.Debug($"Clearing layout engine {Name}");
		_stack.Clear();
	}

	public bool Contains(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window.Title}");
		return _stack.Any(x => x.Handle == window.Handle);
	}

	public void CopyTo(IWindow[] array, int arrayIndex)
	{
		Logger.Debug($"Copying layout engine {Name} to array");
		_stack.CopyTo(array, arrayIndex);
	}

	public IEnumerator<IWindow> GetEnumerator() => _stack.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation location);

	public IWindow? GetFirstWindow()
	{
		return _stack.FirstOrDefault();
	}

	/// <summary>
	/// Focus the window in the given direction.
	/// Stack layout engines should only implement <see cref="WindowDirection.Left"/>
	/// and <see cref="WindowDirection.Right"/>.
	public void FocusWindowInDirection(WindowDirection direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in layout engine {Name}");

		if (direction != WindowDirection.Left && direction != WindowDirection.Right)
		{
			return;
		}

		// Find the index of the window in the stack
		int windowIndex = _stack.FindIndex(x => x.Handle == window.Handle);
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window.Title} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		IWindow adjWindow = _stack[adjIndex];
		adjWindow.Focus();
	}

	/// <summary>
	/// Swap the window with the window in the given direction.
	/// Stack layout engines should only implement <see cref="WindowDirection.Left"/>
	/// and <see cref="WindowDirection.Right"/>.
	/// </summary>
	public void SwapWindowInDirection(WindowDirection direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in layout engine {Name}");

		if (direction != WindowDirection.Left && direction != WindowDirection.Right)
		{
			return;
		}

		// Find the index of the window in the stack
		int windowIndex = _stack.FindIndex(x => x.Handle == window?.Handle);
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window?.Title} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		// Swap window
		IWindow adjWindow = _stack[adjIndex];
		_stack[windowIndex] = adjWindow;
		_stack[adjIndex] = window;
	}

	/// <summary>
	/// Gets the delta to determine whether we want to move towards 0 or not.
	/// </summary>
	/// <param name="leftToRight">Whether we are moving left to right or right to left.</param>
	/// <param name="direction">The window direction to move.</param>
	private static int GetDelta(bool leftToRight, WindowDirection direction)
	{
		if (leftToRight)
		{
			return direction == WindowDirection.Left ? -1 : 1;
		}
		else
		{
			return direction == WindowDirection.Left ? 1 : -1;
		}
	}

	/// <summary>
	/// Shrink the primary area of the layout engine.
	/// </summary>
	public void ShrinkPrimaryArea()
	{
		Logger.Debug($"Shrinking primary area of layout engine {Name}");
		_primaryAreaSizePercentOffset += _primaryAreaSizePercentIncrement;
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Expand the primary area of the layout engine.
	/// </summary>
	public void ExpandPrimaryArea()
	{
		Logger.Debug($"Expanding primary area of layout engine {Name}");
		_primaryAreaSizePercentOffset -= _primaryAreaSizePercentIncrement;
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Reset the primary area of the layout engine.
	/// </summary>
	public void ResetPrimaryArea()
	{
		Logger.Debug($"Resetting primary area of layout engine {Name}");
		_primaryAreaSizePercentOffset = 0;
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Increment the number of windows in the primary area of the layout engine.
	/// </summary>
	public void IncrementNumInPrimaryArea()
	{
		Logger.Debug($"Incrementing number of windows in primary area of layout engine {Name}");
		_primaryAreaCountOffset++;
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Decrement the number of windows in the primary area of the layout engine.
	/// </summary>
	public void DecrementNumInPrimaryArea()
	{
		Logger.Debug($"Decrementing number of windows in primary area of layout engine {Name}");

		if (_primaryAreaTotal > 1)
		{
			_primaryAreaCountOffset--;
			_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
		}
	}
}
