using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

[Slice(Path = "/", Key = "Window")]
internal partial class WindowSlice
{
	private readonly Dictionary<HWND, IWindow> _data = new();

	//public void Dispatch<T>(Transform<T> storeAction) => throw new NotImplementedException();

	//public TResult Pick<TResult>(Func<IReadOnlyDictionary<HWND, IWindow>, TResult> selector) => selector(_data);

	[Transformer]
	private void AddTestData(int a, int b)
	{
		int c = a + b;
	}
}
