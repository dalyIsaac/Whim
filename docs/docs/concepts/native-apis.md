# Native APIs

Whim uses the [CsWin32](https://github.com/microsoft/CsWin32) source generator to add Win32 P/Invoke methods and types. The list of items added can be found in `NativeMethods.txt`.

Most APIs are `internal` to Whim, with the exception of APIs which:

- have been redefined manually with the `public` access modifier, like <xref:Windows.Win32.Foundation.HWND>
- APIs which are exposed as part of <xref:Whim.INativeManager> (via the <xref:Whim.IContext>)
