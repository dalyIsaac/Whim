using System;
using System.Collections.Generic;
using System.ComponentModel;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Whim.TestUtils;

/// <summary>
/// Exception thrown when an event is raised when it should not have been.
/// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors
public class ShouldNotRaiseException : XunitException
#pragma warning restore CA1032 // Implement standard exception constructors
{
	/// <summary>
	/// Creates a new instance of the <see cref="ShouldNotRaiseException"/> class.
	/// </summary>
	/// <param name="type"></param>
	public ShouldNotRaiseException(Type type)
		: base($"Expected event of type {type} to not be raised.") { }
}

/// <summary>
/// Exception thrown when an event is raised when it should have been.
/// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors
public class ShouldRaiseException : XunitException
#pragma warning restore CA1032 // Implement standard exception constructors
{
	/// <summary>
	/// Creates a new instance of the <see cref="ShouldRaiseException"/> class.
	/// </summary>
	/// <param name="type"></param>
	public ShouldRaiseException(Type type)
		: base($"Expected event of type {type} to be raised.") { }
}

/// <summary>
/// Class containing methods with custom assertions.
/// </summary>
public static class CustomAssert
{
	/// <summary>
	/// Asserts that an event is not raised.
	/// </summary>
	/// <typeparam name="T">The type of event to check.</typeparam>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <exception cref="ShouldNotRaiseException">Thrown when the event is raised.</exception>
	public static void DoesNotRaise<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action action)
	{
		bool raised = false;
		void handler(object? sender, T e)
		{
			raised = true;
		}
		attach(handler);
		try
		{
			action();
		}
		finally
		{
			detach(handler);
		}

		if (raised)
		{
			throw new ShouldNotRaiseException(typeof(T));
		}
	}

	/// <summary>
	/// Asserts that <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised.
	/// </summary>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <exception cref="ShouldNotRaiseException">Thrown when the event is raised.</exception>
	public static void DoesNotPropertyChange(
		Action<PropertyChangedEventHandler> attach,
		Action<PropertyChangedEventHandler> detach,
		Action action
	)
	{
		bool raised = false;
		void handler(object? sender, PropertyChangedEventArgs e)
		{
			raised = true;
		}
		attach(handler);
		try
		{
			action();
		}
		finally
		{
			detach(handler);
		}

		if (raised)
		{
			throw new ShouldNotRaiseException(typeof(PropertyChangedEventArgs));
		}
	}

	public static void NoContextCalls(IContext ctx)
	{
		Assert.Empty(ctx.ResourceManager.ReceivedCalls());
		Assert.Empty(ctx.WorkspaceManager.ReceivedCalls());
		Assert.Empty(ctx.WindowManager.ReceivedCalls());
		Assert.Empty(ctx.MonitorManager.ReceivedCalls());
		Assert.Empty(ctx.RouterManager.ReceivedCalls());
		Assert.Empty(ctx.FilterManager.ReceivedCalls());
		Assert.Empty(ctx.CommandManager.ReceivedCalls());
		Assert.Empty(ctx.KeybindManager.ReceivedCalls());
		Assert.Empty(ctx.PluginManager.ReceivedCalls());
		Assert.Empty(ctx.NativeManager.ReceivedCalls());
		Assert.Empty(ctx.FileManager.ReceivedCalls());
		Assert.Empty(ctx.NotificationManager.ReceivedCalls());
	}

	/// <summary>
	/// Asserts that an event is raised.
	/// </summary>
	/// <typeparam name="T">The type of event to check.</typeparam>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <param name="customHandler">A custom handler to call when the event is raised.</param>
	/// <exception cref="ShouldRaiseException">Thrown when the event is not raised.</exception>
	public static void Raises<T>(
		Action<EventHandler<T>> attach,
		Action<EventHandler<T>> detach,
		Action action,
		EventHandler<T> customHandler
	)
	{
		bool raised = false;
		void handler(object? sender, T e)
		{
			raised = true;
			customHandler(sender, e);
		}

		attach(handler);
		try
		{
			action();
		}
		finally
		{
			detach(handler);
		}

		if (!raised)
		{
			throw new ShouldRaiseException(typeof(T));
		}
	}

	/// <summary>
	/// Asserts that the <see cref="WorkspaceLayoutCompletedEventArgs"/> is raised with the expected workspace.
	/// </summary>
	/// <param name="rootSector">The root sector.</param>
	/// <param name="action">The action to perform.</param>
	/// <param name="expectedWorkspace">The expected workspace.</param>
	internal static void Layout(
		MutableRootSector rootSector,
		Action action,
		Guid[]? layoutWorkspaceIds = null,
		Guid[]? noLayoutWorkspaceIds = null
	)
	{
		// Populate the dictionaries with the remaining workspace ids for each event.
		Dictionary<Guid, int> workspaceStartedRemainingIds = new();
		foreach (Guid id in layoutWorkspaceIds ?? Array.Empty<Guid>())
		{
			workspaceStartedRemainingIds[id] = workspaceStartedRemainingIds.GetValueOrDefault(id, 0) + 1;
		}

		Dictionary<Guid, int> workspaceCompletedRemainingIds = new();
		foreach (Guid id in layoutWorkspaceIds ?? Array.Empty<Guid>())
		{
			workspaceCompletedRemainingIds[id] = workspaceCompletedRemainingIds.GetValueOrDefault(id, 0) + 1;
		}

		bool layoutStarted = false;
		bool layoutCompleted = false;

		// Event handlers for decreasing the remaining workspace ids.
		void WorkspaceLayoutStarted(object? sender, WorkspaceLayoutStartedEventArgs e)
		{
			layoutStarted = true;
			workspaceStartedRemainingIds[e.Workspace.Id] =
				workspaceStartedRemainingIds.GetValueOrDefault(e.Workspace.Id, 0) - 1;
		}

		void WorkspaceLayoutCompleted(object? sender, WorkspaceLayoutCompletedEventArgs e)
		{
			layoutCompleted = true;
			workspaceCompletedRemainingIds[e.Workspace.Id] =
				workspaceCompletedRemainingIds.GetValueOrDefault(e.Workspace.Id, 0) - 1;
		}

		// Perform the action.
		rootSector.WorkspaceSector.WorkspaceLayoutStarted += WorkspaceLayoutStarted;
		rootSector.WorkspaceSector.WorkspaceLayoutCompleted += WorkspaceLayoutCompleted;
		try
		{
			action();
		}
		finally
		{
			rootSector.WorkspaceSector.WorkspaceLayoutStarted -= WorkspaceLayoutStarted;
			rootSector.WorkspaceSector.WorkspaceLayoutCompleted -= WorkspaceLayoutCompleted;
		}

		// Assert that the remaining workspace ids are 0.
		foreach (Guid id in layoutWorkspaceIds ?? Array.Empty<Guid>())
		{
			int remaining = workspaceStartedRemainingIds[id];
			if (remaining != 0)
			{
				throw new Exception($"Workspace {id}'s layout was not started {remaining} times.");
			}
		}

		foreach (Guid id in layoutWorkspaceIds ?? Array.Empty<Guid>())
		{
			int remaining = workspaceCompletedRemainingIds[id];
			if (remaining != 0)
			{
				throw new Exception($"Workspace {id}'s layout was not completed {remaining} times.");
			}
		}

		foreach (Guid id in noLayoutWorkspaceIds ?? Array.Empty<Guid>())
		{
			if (workspaceCompletedRemainingIds.ContainsKey(id))
			{
				throw new Exception($"Workspace {id} was unexpectedly laid out.");
			}
		}

		// Handle for when neither lists were provided.
		if (layoutWorkspaceIds == null && noLayoutWorkspaceIds == null && !layoutStarted && !layoutCompleted)
		{
			throw new Exception("No events were raised.");
		}
	}

	/// <summary>
	/// Get the stored transforms from the <see cref="StoreWrapper"/>.
	/// </summary>
	public static List<object> GetTransforms(this IContext ctx) => ((StoreWrapper)ctx.Store).Transforms;

	/// <summary>
	/// Asserts that the enumerable contains the expected number of items.
	/// </summary>
	/// <param name="enumerable">The enumerable.</param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="count">The expected count.</param>
	public static void Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, int count)
	{
		int countFound = 0;
		foreach (T item in enumerable)
		{
			if (predicate(item))
			{
				countFound++;
			}
		}

		if (countFound != count)
		{
			throw new Exception($"Expected {count} items, found {countFound}");
		}
	}
}
