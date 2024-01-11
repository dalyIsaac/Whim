using System;
using System.ComponentModel;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Whim.TestUtils;

/// <summary>
/// Exception thrown when an event is raised when it should not have been.
/// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors
public class DoesNotRaiseException : XunitException
#pragma warning restore CA1032 // Implement standard exception constructors
{
	/// <summary>
	/// Creates a new instance of the <see cref="DoesNotRaiseException"/> class.
	/// </summary>
	/// <param name="type"></param>
	public DoesNotRaiseException(Type type)
		: base($"Expected event of type {type} to not be raised.") { }
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
	/// <exception cref="DoesNotRaiseException">Thrown when the event is raised.</exception>
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
			throw new DoesNotRaiseException(typeof(T));
		}
	}

	/// <summary>
	/// Asserts that <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised.
	/// </summary>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <exception cref="DoesNotRaiseException">Thrown when the event is raised.</exception>
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
			throw new DoesNotRaiseException(typeof(PropertyChangedEventArgs));
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
		Assert.Empty(internalCtx.MonitorManager.ReceivedCalls());
		Assert.Empty(internalCtx.WindowManager.ReceivedCalls());
		Assert.Empty(internalCtx.KeybindHook.ReceivedCalls());
		Assert.Empty(internalCtx.MouseHook.ReceivedCalls());
		Assert.Empty(internalCtx.DeferWindowPosManager.ReceivedCalls());
	}
}
