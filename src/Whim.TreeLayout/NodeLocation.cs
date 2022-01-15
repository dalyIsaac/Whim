namespace Whim.TreeLayout;

public class NodeLocation : ILocation<double>
{
	public double X { get; set; }
	public double Y { get; set; }
	public double Width { get; set; }
	public double Height { get; set; }

	public bool IsPointInside(double x, double y) => ILocation<double>.IsPointInside(this, x, y);
}
