using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The transformers for the <see cref="IStoreData.Windows"/> slice.
/// </summary>
public static class WindowSliceTransformers
{
	private const string _sliceKey = "windows";

	/// <summary>
	/// Add a window to the <see cref="IStoreData.Windows"/> slice.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static Transform<IWindow> AddWindow(IWindow window) => new($"{_sliceKey}/AddWindow", window);
}

internal class WindowSlice : ISlice<IReadOnlyDictionary<HWND, IWindow>>
{
	private readonly Dictionary<HWND, IWindow> _data = new();

	public void Dispatch<T>(Transform<T> storeAction) => throw new NotImplementedException();

	public TResult Pick<TResult>(Func<IReadOnlyDictionary<HWND, IWindow>, TResult> selector) => selector(_data);
}
