using System;
using System.ComponentModel;
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
	/// Asserts that a property changed event is not raised for a property.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="propertyName"></param>
	/// <param name="action"></param>
	/// <exception cref="DoesNotRaiseException"></exception>
	public static void PropertyNotChanged(INotifyPropertyChanged item, string propertyName, Action action)
	{
		bool raised = false;
		void handler(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == propertyName)
			{
				raised = true;
			}
		}
		item.PropertyChanged += handler;
		try
		{
			action();
		}
		finally
		{
			item.PropertyChanged -= handler;
		}

		if (raised)
		{
			throw new DoesNotRaiseException(typeof(PropertyChangedEventArgs));
		}
	}
}
