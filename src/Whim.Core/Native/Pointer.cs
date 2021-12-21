using Windows.Win32.Foundation;

namespace Whim.Core;

/// <summary>
/// A simple wrapper for <see cref="HWND"/>, which includes a <see cref="ToString"/> implementation.
/// </summary>
internal class Pointer
{
	public HWND Handle { get; }

	internal Pointer(HWND handle)
	{
		Handle = handle;
	}

	public override string ToString()
	{
		return Handle.Value.ToString();
	}
}
