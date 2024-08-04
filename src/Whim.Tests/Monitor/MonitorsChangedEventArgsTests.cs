namespace Whim.Tests;

public class MonitorsChangedEventArgsTests
{
	[Fact]
	public void CurrentMonitors_ReturnsSameInstance()
	{
		// Given
		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = [],
				RemovedMonitors = [],
				AddedMonitors = [],
			};

		// When
		IEnumerable<IMonitor> currentMonitors1 = args.CurrentMonitors;
		IEnumerable<IMonitor> currentMonitors2 = args.CurrentMonitors;

		// Then
		Assert.Same(currentMonitors1, currentMonitors2);
	}

	[Theory, AutoSubstituteData]
	public void CurrentMonitors_Concat(
		IMonitor unchangedMonitor1,
		IMonitor unchangedMonitor2,
		IMonitor addedMonitor1,
		IMonitor addedMonitor2
	)
	{
		// Given
		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = [unchangedMonitor1, unchangedMonitor2],
				RemovedMonitors = [],
				AddedMonitors = [addedMonitor1, addedMonitor2],
			};

		// When
		IEnumerable<IMonitor> currentMonitors = args.CurrentMonitors;

		// Then
		Assert.Collection(
			currentMonitors,
			monitor => Assert.Same(unchangedMonitor1, monitor),
			monitor => Assert.Same(unchangedMonitor2, monitor),
			monitor => Assert.Same(addedMonitor1, monitor),
			monitor => Assert.Same(addedMonitor2, monitor)
		);
	}

	[Theory, AutoSubstituteData]
	public void PreviousMonitors_Concat(
		IMonitor unchangedMonitor1,
		IMonitor unchangedMonitor2,
		IMonitor removedMonitor1,
		IMonitor removedMonitor2
	)
	{
		// Given


		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = [unchangedMonitor1, unchangedMonitor2],
				RemovedMonitors = [removedMonitor1, removedMonitor2],
				AddedMonitors = [],
			};

		// When
		IEnumerable<IMonitor> previousMonitors = args.PreviousMonitors;

		// Then
		Assert.Collection(
			previousMonitors,
			monitor => Assert.Same(unchangedMonitor1, monitor),
			monitor => Assert.Same(unchangedMonitor2, monitor),
			monitor => Assert.Same(removedMonitor1, monitor),
			monitor => Assert.Same(removedMonitor2, monitor)
		);
	}
}
