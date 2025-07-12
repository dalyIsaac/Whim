namespace Whim.Tests;

public class MonitorWorkspaceChangedEventTests
{
	[Theory, AutoSubstituteData]
	public void Equals_Null(IMonitor monitor, IWorkspace workspace)
	{
		// Given
		MonitorWorkspaceChangedEventArgs a = new() { CurrentWorkspace = workspace, Monitor = monitor };

		// Then
#pragma warning disable CA1508 // Avoid dead conditional code
		Assert.False(a.Equals(null));
#pragma warning restore CA1508 // Avoid dead conditional code
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentType(IMonitor monitor, IWorkspace workspace)
	{
		// Given
		MonitorWorkspaceChangedEventArgs a = new() { CurrentWorkspace = workspace, Monitor = monitor };

		// Then
		Assert.False(a.Equals(true));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentMonitor(IMonitor aMonitor, IWorkspace workspace, IMonitor bMonitor)
	{
		// Given
		aMonitor.Handle.Returns((HMONITOR)1);
		bMonitor.Handle.Returns((HMONITOR)2);
		MonitorWorkspaceChangedEventArgs a = new() { Monitor = aMonitor, CurrentWorkspace = workspace };
		MonitorWorkspaceChangedEventArgs b = new() { Monitor = bMonitor, CurrentWorkspace = workspace };

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentPreviousWorkspace(IContext ctx, IMonitor monitor)
	{
		// Given
		IWorkspace workspace = StoreTestUtils.CreateWorkspace();
		IWorkspace aWorkspace = StoreTestUtils.CreateWorkspace();
		IWorkspace bWorkspace = StoreTestUtils.CreateWorkspace();
		MonitorWorkspaceChangedEventArgs a = new()
		{
			Monitor = monitor,
			CurrentWorkspace = workspace,
			PreviousWorkspace = aWorkspace,
		};
		MonitorWorkspaceChangedEventArgs b = new()
		{
			Monitor = monitor,
			CurrentWorkspace = workspace,
			PreviousWorkspace = bWorkspace,
		};

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentCurrentWorkspace(IContext ctx, IMonitor monitor)
	{
		// Given
		IWorkspace aCurrentWorkspace = StoreTestUtils.CreateWorkspace();
		IWorkspace bCurrentWorkspace = StoreTestUtils.CreateWorkspace();
		IWorkspace previousWorkspace = StoreTestUtils.CreateWorkspace();
		MonitorWorkspaceChangedEventArgs a = new()
		{
			Monitor = monitor,
			CurrentWorkspace = aCurrentWorkspace,
			PreviousWorkspace = bCurrentWorkspace,
		};
		MonitorWorkspaceChangedEventArgs b = new()
		{
			Monitor = monitor,
			CurrentWorkspace = aCurrentWorkspace,
			PreviousWorkspace = previousWorkspace,
		};

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_Sucess(IMonitor monitor, IWorkspace currentWorkspace, IWorkspace previousWorkspace)
	{
		// Given
		MonitorWorkspaceChangedEventArgs a = new()
		{
			Monitor = monitor,
			CurrentWorkspace = currentWorkspace,
			PreviousWorkspace = previousWorkspace,
		};
		MonitorWorkspaceChangedEventArgs b = new()
		{
			Monitor = monitor,
			CurrentWorkspace = currentWorkspace,
			PreviousWorkspace = previousWorkspace,
		};

		// Then
		Assert.True(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void GetHashCode_Success(IMonitor monitor, IWorkspace currentWorkspace, IWorkspace previousWorkspace)
	{
		// Given
		MonitorWorkspaceChangedEventArgs a = new()
		{
			Monitor = monitor,
			CurrentWorkspace = currentWorkspace,
			PreviousWorkspace = previousWorkspace,
		};

		// Then
		Assert.Equal(HashCode.Combine(monitor, previousWorkspace, currentWorkspace), a.GetHashCode());
	}
}
