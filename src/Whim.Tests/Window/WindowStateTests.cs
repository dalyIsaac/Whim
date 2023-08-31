using Moq;
using Xunit;

namespace Whim.Tests;

public class WindowStateTests
{
	[Fact]
	public void Equals_NotWindowState()
	{
		// Given
		WindowState windowState =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = new Mock<IWindow>().Object
			};

		// When
		bool result = windowState.Equals(new object());

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Equals_DifferentLocation()
	{
		// Given
		Mock<IWindow> windowMock = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>() { X = 1, Y = 1 },
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Equals_DifferentWindowSize()
	{
		// Given
		Mock<IWindow> windowMock = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Maximized,
				Window = windowMock.Object
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Equals_DifferentWindow()
	{
		// Given
		Mock<IWindow> windowMock1 = new();
		Mock<IWindow> windowMock2 = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock1.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock2.Object
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Equals_Success()
	{
		// Given
		Mock<IWindow> windowMock = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.True(result);
	}

	[Fact]
	public void Equals_Operator_Success()
	{
		// Given
		Mock<IWindow> windowMock = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock.Object
			};

		// When
		bool result = windowState1 == windowState2;

		// Then
		Assert.True(result);
	}

	[Fact]
	public void NotEquals_Operator_Success()
	{
		// Given
		Mock<IWindow> windowMock1 = new();
		Mock<IWindow> windowMock2 = new();
		WindowState windowState1 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock1.Object
			};
		WindowState windowState2 =
			new()
			{
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal,
				Window = windowMock2.Object
			};

		// When
		bool result = windowState1 != windowState2;

		// Then
		Assert.True(result);
	}
}
