namespace Whim.TreeLayout.Tests;

internal static class NodeLocationExtensions
{
	public static Location Scale(this ILocation<double> node, ILocation<int> scale) => new((int)Math.Round(node.X * scale.Width),
																							 (int)Math.Round(node.Y * scale.Height),
																							 (int)Math.Round(node.Width * scale.Width),
																							 (int)Math.Round(node.Height * scale.Height));
}
