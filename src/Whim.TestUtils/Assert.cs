using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
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

	internal static void NoInternalContextCalls(IInternalContext internalCtx)
	{
		Assert.Empty(internalCtx.CoreSavedStateManager.ReceivedCalls());
		Assert.Empty(internalCtx.CoreNativeManager.ReceivedCalls());
		Assert.Empty(internalCtx.WindowMessageMonitor.ReceivedCalls());
		Assert.Empty(internalCtx.WindowManager.ReceivedCalls());
		Assert.Empty(internalCtx.KeybindHook.ReceivedCalls());
		Assert.Empty(internalCtx.MouseHook.ReceivedCalls());
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
		IEnumerable<Guid>? layoutWorkspaceIds = null,
		IEnumerable<Guid>? noLayoutWorkspaceIds = null
	)
	{
		Dictionary<Guid, int> workspaceStartedRemainingIds = new();
		foreach (Guid id in layoutWorkspaceIds ?? Enumerable.Empty<Guid>())
		{
			workspaceStartedRemainingIds[id] = workspaceStartedRemainingIds.GetValueOrDefault(id, 0) + 1;
		}

		Dictionary<Guid, int> workspaceCompletedRemainingIds = new();
		foreach (Guid id in layoutWorkspaceIds ?? Enumerable.Empty<Guid>())
		{
			workspaceCompletedRemainingIds[id] = workspaceCompletedRemainingIds.GetValueOrDefault(id, 0) + 1;
		}

		Raises<WorkspaceLayoutCompletedEventArgs>(
			h => rootSector.WorkspaceSector.WorkspaceLayoutCompleted += h,
			h => rootSector.WorkspaceSector.WorkspaceLayoutCompleted -= h,
			() =>
			{
				Raises<WorkspaceLayoutStartedEventArgs>(
					h => rootSector.WorkspaceSector.WorkspaceLayoutStarted += h,
					h => rootSector.WorkspaceSector.WorkspaceLayoutStarted -= h,
					() => action(),
					(sender, e) =>
					{
						workspaceStartedRemainingIds[e.Workspace.Id] =
							workspaceStartedRemainingIds.GetValueOrDefault(e.Workspace.Id, 0) - 1;
					}
				);
			},
			(sender, e) =>
			{
				workspaceCompletedRemainingIds[e.Workspace.Id] =
					workspaceCompletedRemainingIds.GetValueOrDefault(e.Workspace.Id, 0) - 1;
			}
		);

		foreach ((Guid id, int remaining) in workspaceStartedRemainingIds)
		{
			if (remaining > 0)
			{
				throw new Exception($"Workspace {id} was not started {remaining} times.");
			}
		}

		foreach ((Guid id, int remaining) in workspaceCompletedRemainingIds)
		{
			if (remaining > 0)
			{
				throw new Exception($"Workspace {id} was not completed {remaining} times.");
			}
		}
	}

	/// <summary>
	/// Get the stored transforms from the <see cref="StoreWrapper"/>.
	/// </summary>
	public static List<object> GetTransforms(this IContext ctx) => ((StoreWrapper)ctx.Store).Transforms;
}
