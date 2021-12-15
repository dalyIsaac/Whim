namespace Whim.Core;

public class Area : IArea
{
	public int Width { get; }
	public int Height { get; }

	public Area(int width, int height)
	{
		Width = width;
		Height = height;
	}
}
