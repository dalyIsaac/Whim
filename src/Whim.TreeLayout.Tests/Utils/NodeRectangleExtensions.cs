namespace Whim.TreeLayout.Tests;

internal static class NodeRectangleExtensions
{
	public static Rectangle<int> Scale(this IRectangle<double> node, IRectangle<int> scale)
	{
		return new Rectangle<int>
		{
			X = (int)Math.Round(node.X * scale.Width),
			Y = (int)Math.Round(node.Y * scale.Height),
			Width = (int)Math.Round(node.Width * scale.Width),
			Height = (int)Math.Round(node.Height * scale.Height)
		};
	}
}
