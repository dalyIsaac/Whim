using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
public abstract class BaseProxyLayoutEngine : ILayoutEngine
{
	protected readonly ILayoutEngine _innerLayoutEngine;
	public Commander Commander { get; } = new();

	public BaseProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
	{
		_innerLayoutEngine = innerLayoutEngine;
	}

	/// <summary>
	/// The name is only really important for the user, so we can use the name of the proxied layout engine.
	/// </summary>
	public string Name => _innerLayoutEngine.Name;

	public abstract void AddWindow(IWindow window);

	public abstract bool RemoveWindow(IWindow window);

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation location);
}
