using System.Collections.Immutable;

namespace Whim.SliceLayout;

// TODO: docs
public interface IArea
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	bool IsHorizontal { get; }
}

public record BaseArea : IArea
{
	public bool IsHorizontal { get; init; }
}

public record SliceArea : BaseArea
{
	public uint Priority { get; init; }

	public uint MaxChildren { get; init; }

	public ImmutableList<double> Weights { get; }

	public ImmutableList<IArea> Children { get; }

	public SliceArea(params (double Weight, IArea Child)[] children)
	{
		ImmutableList<double>.Builder weightsBuilder = ImmutableList.CreateBuilder<double>();
		ImmutableList<IArea>.Builder childrenBuilder = ImmutableList.CreateBuilder<IArea>();

		foreach ((double weight, IArea child) in children)
		{
			weightsBuilder.Add(weight);
			childrenBuilder.Add(child);
		}

		Weights = weightsBuilder.ToImmutable();
		Children = childrenBuilder.ToImmutable();
	}
}
