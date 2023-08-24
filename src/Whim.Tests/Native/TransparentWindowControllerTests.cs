using Moq;
using Xunit;

namespace Whim;

public class TransparentWindowController
{
	private class Wrapper
	{
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public Mock<IContext> Context { get; } = new();
	}

	[Fact]
	public void WindowProc_EraseBackground_CannotGetClientRect()
	{
		// TODO
	}

	[Fact]
	public void WindowProc_EraseBackground_Success()
	{
		// TODO
	}

	[Fact]
	public void WindowProc_DwmCompositionChanged_Success()
	{
		// TODO
	}

	[Fact]
	public void WindowProc_Rest()
	{
		// TODO
	}

	[Fact]
	public void Dispose_Success()
	{
		// TODO
	}

	[Fact]
	public void Dispose_Finalizer_Success()
	{
		// TODO
	}

	[Fact]
	public void Dispose_Twice()
	{
		// TODO
	}
}
