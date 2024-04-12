using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

///// <summary>
///// The root of the store data.
///// </summary>
//public interface IStoreData
//{
//	/// <summary>
//	/// The <see cref="IWindow"/> slice.
//	/// </summary>
//	ISlice<IReadOnlyDictionary<HWND, IWindow>> Windows { get; }
//}

//internal class StoreData : IStoreData
//{
//	// TODO: Lock?
//	public ISlice<IReadOnlyDictionary<HWND, IWindow>> Windows { get; } = new WindowSlice();
//}

//internal class Store : ISlice<IStoreData>
//{
//	private readonly IStoreData _storeData = new StoreData();

//	public void Dispatch<T>(Transform<T> storeAction) => throw new NotImplementedException();

//	public TResult Pick<TResult>(Func<IStoreData, TResult> selector) => throw new NotImplementedException();
//}

//internal static class TestTemp
//{
//	static void Test()
//	{
//		Store store = new();
//		// store.Select(static (store) => store.Windows.)
//	}
//}
