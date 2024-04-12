using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

[Slice(Path = "/", Key = "Window")]
internal partial class WindowSlice
{
	[SliceData]
	private readonly Dictionary<HWND, IWindow> _data = new();

	[Transformer]
	private void AddTestData(int a, int b)
	{
		int c = a + b;
	}
}
