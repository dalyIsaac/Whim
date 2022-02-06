using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetAdjacentNode
{
	private readonly TestTreeEngine _testEngine = new();
	private readonly Mock<IMonitor> _monitor;

	public TestGetAdjacentNode()
	{
		Logger.Initialize();

		_monitor = new();
		_monitor.Setup(m => m.Width).Returns(1920);
		_monitor.Setup(m => m.Height).Returns(1080);
	}

		#region GetAdjacentNode
	/// <summary>
	/// There is no node to the left of Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_Left()
	{
		// The root should be a split node, with two children.
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.LeftNode, WindowDirection.Left, _monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightBottom is Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Left()
	{
		Assert.Equal(_testEngine.LeftNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, WindowDirection.Left, _monitor.Object));
	}

	/// <summary>
	/// The node above RightBottom should be RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Up()
	{
		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, WindowDirection.Up, _monitor.Object));
	}

	/// <summary>
	/// There is no node to the right or below RightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Null()
	{
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, WindowDirection.Right, _monitor.Object));
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, WindowDirection.Down, _monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightTopRight3 is RightTopLeftBottomRightTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopRight3_Left()
	{
		Assert.Equal(_testEngine.RightTopLeftBottomRightTopNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightTopRight3Node, WindowDirection.Left, _monitor.Object));
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopRight2.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Right()
	{
		Assert.Equal(_testEngine.RightTopRight2Node, _testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, WindowDirection.Right, _monitor.Object));
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopLeftBottomRightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Down()
	{
		Assert.Equal(_testEngine.RightTopLeftBottomRightBottomNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, WindowDirection.Down, _monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightTopLeftBottomRightTop is RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Left()
	{
		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, WindowDirection.Left, _monitor.Object));
	}

	/// <summary>
	/// The node above RightTopLeftBottomRightTop is RightTopLeftTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Up()
	{
		Assert.Equal(_testEngine.RightTopLeftTopNode, _testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, WindowDirection.Up, _monitor.Object));
	}
	#endregion

}
