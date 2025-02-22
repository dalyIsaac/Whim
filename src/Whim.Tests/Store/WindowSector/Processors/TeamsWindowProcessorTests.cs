using NSubstitute;
using Whim;
using Xunit;

public class TeamsWindowProcessorTests
{
	[Theory, AutoSubstituteData]
	public void Create_Failure(IContext ctx, IWindow window)
	{
		// Given a window which is not a Teams window
		window.ProcessFileName.Returns("not-ms-teams.exe");

		// When Create is called
		IWindowProcessor? processor = TeamsWindowProcessor.Create(ctx, window);

		// Then the processor should be null
		Assert.Null(processor);
	}

	[Theory, AutoSubstituteData]
	public void Create_Success(IContext ctx, IWindow window)
	{
		// Given a window which is a Teams window
		window.ProcessFileName.Returns("ms-teams.exe");

		// When Create is called
		IWindowProcessor? processor = TeamsWindowProcessor.Create(ctx, window);

		// Then the processor should not be null and its Window property equals the window
		Assert.NotNull(processor);
		Assert.Equal(window, processor!.Window);
	}

	[Theory, AutoSubstituteData]
	public void ProcessEvent_MinimizesWindow_WhenTitleStartsWithMeetingCompact(IContext ctx, IWindow window)
	{
		// Given a Teams window with a title starting with "Meeting compact view"
		window.ProcessFileName.Returns("ms-teams.exe");
		window.Title.Returns("Meeting compact view - Sample Meeting");

		IWindowProcessor processor = TeamsWindowProcessor.Create(ctx, window)!;

		// When ProcessEvent is called
		WindowProcessorResult result = processor.ProcessEvent(0, 0, 0, 0, 0);

		// Then the window should be minimized and result should be Ignore
		window.Received(1).ShowMinimized();
		Assert.Equal(WindowProcessorResult.Ignore, result);
	}

	[Theory, AutoSubstituteData]
	public void ProcessEvent_DoesNotMinimizeWindow_WhenTitleDoesNotMatch(IContext ctx, IWindow window)
	{
		// Given a Teams window with a title not starting with "Meeting compact view"
		window.ProcessFileName.Returns("ms-teams.exe");
		window.Title.Returns("Regular Meeting");

		IWindowProcessor processor = TeamsWindowProcessor.Create(ctx, window)!;

		// When ProcessEvent is called
		WindowProcessorResult result = processor.ProcessEvent(0, 0, 0, 0, 0);

		// Then the window should not be minimized and result should be Process
		window.DidNotReceive().ShowMinimized();
		Assert.Equal(WindowProcessorResult.Process, result);
	}
}
