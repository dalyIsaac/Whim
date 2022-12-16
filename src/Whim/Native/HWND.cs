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
		public readonly struct HWND : IEquatable<HWND>
		{
			/// <summary>
			///
			/// </summary>
			public readonly IntPtr Value;

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
			public static explicit operator HWND(IntPtr value) => new HWND(value);

			/// <inheritdoc/>
			public static bool operator ==(HWND left, HWND right) => left.Value == right.Value;

			/// <inheritdoc/>
			public static bool operator !=(HWND left, HWND right) => !(left == right);

			/// <inheritdoc/>
			public bool Equals(HWND other) => this.Value == other.Value;

			/// <inheritdoc/>
			public override bool Equals(object? obj) => obj is HWND other && this.Equals(other);

			/// <inheritdoc/>
			public override int GetHashCode() => this.Value.GetHashCode();
		}
	}
}
