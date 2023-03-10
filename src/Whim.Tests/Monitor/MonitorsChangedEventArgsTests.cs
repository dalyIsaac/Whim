using Moq;
using System;
using System.Collections.Generic;
using Xunit;

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
				UnchangedMonitors = Array.Empty<IMonitor>(),
				RemovedMonitors = Array.Empty<IMonitor>(),
				AddedMonitors = Array.Empty<IMonitor>(),
			};

		// When
		IEnumerable<IMonitor> currentMonitors1 = args.CurrentMonitors;
		IEnumerable<IMonitor> currentMonitors2 = args.CurrentMonitors;

		// Then
		Assert.Same(currentMonitors1, currentMonitors2);
	}

	[Fact]
	public void CurrentMonitors_Concat()
	{
		// Given
		IMonitor unchangedMonitor1 = Mock.Of<IMonitor>();
		IMonitor unchangedMonitor2 = Mock.Of<IMonitor>();
		IMonitor addedMonitor1 = Mock.Of<IMonitor>();
		IMonitor addedMonitor2 = Mock.Of<IMonitor>();

		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = new[] { unchangedMonitor1, unchangedMonitor2 },
				RemovedMonitors = Array.Empty<IMonitor>(),
				AddedMonitors = new[] { addedMonitor1, addedMonitor2 },
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

	[Fact]
	public void PreviousMonitors_Concat()
	{
		// Given
		IMonitor unchangedMonitor1 = Mock.Of<IMonitor>();
		IMonitor unchangedMonitor2 = Mock.Of<IMonitor>();
		IMonitor removedMonitor1 = Mock.Of<IMonitor>();
		IMonitor removedMonitor2 = Mock.Of<IMonitor>();

		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = new[] { unchangedMonitor1, unchangedMonitor2 },
				RemovedMonitors = new[] { removedMonitor1, removedMonitor2 },
				AddedMonitors = Array.Empty<IMonitor>(),
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
