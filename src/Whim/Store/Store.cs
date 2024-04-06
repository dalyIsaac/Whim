using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Data specifying how to update the <see cref="ISlice{TState}"/>. Analagous to a Redux action.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
/// <param name="Key">The unique string key specifying which update to perform.</param>
/// <param name="Payload">The payload to pass for the transform.</param>
public record Transform<T>(string Key, T Payload);

/// <summary>
/// A "slice" or a specific portion of the overall state.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface ISlice<TState>
{
	/// <summary>
	/// Dispatch a <see cref="Transform{T}"/> to update the <see cref="IStoreData"/> of the <see cref="Store"/>
	/// </summary>
	/// <typeparam name="T">The payload of the <see cref="Transform{T}"/></typeparam>
	/// <param name="transform">The <see cref="Transform{T}"/> to update the state.</param>
	void Dispatch<T>(Transform<T> transform);

	/// <summary>
	/// Extract state from the <typeparamref name="TState"/>. Analagous to a Redux selector.
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	/// <param name="picker"></param>
	/// <returns></returns>
	TResult Pick<TResult>(Func<TState, TResult> picker);
}

/// <summary>
/// The root of the store data.
/// </summary>
public interface IStoreData
{
	/// <summary>
	/// The <see cref="IWindow"/> slice.
	/// </summary>
	ISlice<IReadOnlyDictionary<HWND, IWindow>> Windows { get; }
}

internal class StoreData : IStoreData
{
	// TODO: Lock?
	public ISlice<IReadOnlyDictionary<HWND, IWindow>> Windows { get; } = new WindowSlice();
}

internal class Store : ISlice<IStoreData>
{
	private readonly IStoreData _storeData = new StoreData();

	public void Dispatch<T>(Transform<T> storeAction) => throw new NotImplementedException();

	public TResult Pick<TResult>(Func<IStoreData, TResult> selector) => throw new NotImplementedException();
}

internal static class TestTemp
{
	static void Test()
	{
		Store store = new();
		// store.Select(static (store) => store.Windows.)
	}
}
