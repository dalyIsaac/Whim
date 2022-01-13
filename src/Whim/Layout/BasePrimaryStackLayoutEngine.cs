namespace Whim;

/// <summary>
/// Base layout engine for a stack of windows, but has a primary area.
public abstract class BasePrimaryStackLayoutEngine : BaseStackLayoutEngine
{
	protected readonly int _primaryAreaBaseCount = 1;
	protected int _primaryAreaCountOffset;
	protected int _primaryAreaTotal => _primaryAreaBaseCount + _primaryAreaCountOffset;

	protected readonly int _primaryPercentBase;
	protected int _primaryPercentOffset = 0;
	protected readonly int _primaryPercentIncrement;

	public BasePrimaryStackLayoutEngine(string name, bool leftToRight, int primaryPercent, int primaryPercentIncrement) : base(name, leftToRight)
	{
		_primaryPercentBase = primaryPercent;
		_primaryPercentIncrement = primaryPercentIncrement;
	}

	/// <summary>
	/// Shrink the primary area of the layout engine.
	/// </summary>
	public void ShrinkPrimaryArea()
	{
		Logger.Debug($"Shrinking primary area of layout engine {Name}");
		_primaryPercentOffset += _primaryPercentIncrement;
	}

	/// <summary>
	/// Expand the primary area of the layout engine.
	/// </summary>
	public void ExpandPrimaryArea()
	{
		Logger.Debug($"Expanding primary area of layout engine {Name}");
		_primaryPercentOffset -= _primaryPercentIncrement;
	}

	/// <summary>
	/// Reset the primary area of the layout engine.
	/// </summary>
	public void ResetPrimaryArea()
	{
		Logger.Debug($"Resetting primary area of layout engine {Name}");
		_primaryPercentOffset = 0;
	}

	/// <summary>
	/// Increment the number of windows in the primary area of the layout engine.
	/// </summary>
	public void IncrementNumInPrimaryArea()
	{
		Logger.Debug($"Incrementing number of windows in primary area of layout engine {Name}");
		_primaryAreaCountOffset++;
	}

	/// <summary>
	/// Decrement the number of windows in the primary area of the layout engine.
	/// </summary>
	public void DecrementNumInPrimaryArea()
	{
		Logger.Debug($"Decrementing number of windows in primary area of layout engine {Name}");

		if (_primaryAreaTotal > 0)
		{
			_primaryAreaCountOffset--;
		}
	}
}
