using System;
using System.ComponentModel;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Whim.TestUtils;

/// <summary>
/// Exception thrown when an event is not raises when it should have been (or vice versa).
/// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors
public class ShouldRaiseException : XunitException
#pragma warning restore CA1032 // Implement standard exception constructors
{
	/// <summary>
	/// Creates a new instance of the <see cref="ShouldRaiseException"/> class.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="expected">Whether it was expected that the exception was raised.</param>
	public ShouldRaiseException(Type type, bool expected)
		: base($"Expected event of type {type} to {(expected ? "not" : "")} be raised.") { }
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
	/// <exception cref="ShouldRaiseException">Thrown when the event is raised.</exception>
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
			throw new ShouldRaiseException(typeof(T), expected: false);
		}
	}

	/// <summary>
	/// Asserts that an event is not raised.
	/// </summary>
	/// <typeparam name="T">The type of event to check.</typeparam>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <param name="delayMs">The delay in milliseconds to wait.</param>
	/// <exception cref="ShouldRaiseException">Thrown when the event is raised.</exception>
	public static async Task RaisesAsync<T>(
		Action<EventHandler<T>> attach,
		Action<EventHandler<T>> detach,
		Action action,
		int delayMs
	)
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
			await Task.Delay(delayMs).ConfigureAwait(true);
		}
		finally
		{
			detach(handler);
		}

		if (!raised)
		{
			throw new ShouldRaiseException(typeof(T), expected: true);
		}
	}

	/// <summary>
	/// Asserts that <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised.
	/// </summary>
	/// <param name="attach">The method to attach the event handler.</param>
	/// <param name="detach">The method to detach the event handler.</param>
	/// <param name="action">The action to perform.</param>
	/// <exception cref="ShouldRaiseException">Thrown when the event is raised.</exception>
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
			throw new ShouldRaiseException(typeof(PropertyChangedEventArgs), expected: false);
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
	}
}
