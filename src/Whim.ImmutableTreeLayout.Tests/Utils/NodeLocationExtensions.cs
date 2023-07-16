namespace Whim.ImmutableTreeLayout.Tests;

internal static class NodeLocationExtensions
{
	public static Location<int> Scale(this ILocation<double> node, ILocation<int> scale)
	{
		return new Location<int>
		{
			X = (int)Math.Round(node.X * scale.Width),
			Y = (int)Math.Round(node.Y * scale.Height),
			Width = (int)Math.Round(node.Width * scale.Width),
			Height = (int)Math.Round(node.Height * scale.Height)
		};
	}
}
