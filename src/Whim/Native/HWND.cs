using System;
using System.Diagnostics;

namespace Windows.Win32
{
	namespace Foundation
	{
		/// <summary>
		/// A handle to a window.
		/// </summary>
		[DebuggerDisplay("{Value}")]
		public readonly record struct HWND
		{
			internal readonly IntPtr Value;

			/// <summary>
			/// Creates a new <see cref="HWND"/> from a <see cref="IntPtr"/>.
			/// </summary>
			/// <param name="value"></param>
			public HWND(IntPtr value) => Value = value;

			/// <summary>
			/// Creates a <see langword="null"/> value.
			/// </summary>
			public static HWND Null => default;

			/// <summary>
			/// Whether this <see cref="HWND"/> has a <see langword="null"/> value.
			/// </summary>
			public bool IsNull => Value == default;

			/// <inheritdoc/>
			public static implicit operator IntPtr(HWND value) => value.Value;

			/// <inheritdoc/>
			public static explicit operator HWND(IntPtr value) => new(value);

			/// <inheritdoc/>
			public override string ToString() => Value.ToString();
		}
	}
}
