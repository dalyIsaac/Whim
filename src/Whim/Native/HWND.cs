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
			internal readonly IntPtr Value;

			internal HWND(IntPtr value) => Value = value;

			internal static HWND Null => default;

			/// <summary>
			/// Whether the handle has a zero value.
			/// </summary>
			public bool IsNull => Value == default;

			/// <inheritdoc />
			public static implicit operator IntPtr(HWND value) => value.Value;

			/// <inheritdoc />
			public static explicit operator HWND(IntPtr value) => new(value);

			/// <inheritdoc />
			public static bool operator ==(HWND left, HWND right) => left.Value == right.Value;

			/// <inheritdoc />
			public static bool operator !=(HWND left, HWND right) => !(left == right);

			/// <inheritdoc />
			public bool Equals(HWND other) => Value == other.Value;

			/// <inheritdoc />
			public override bool Equals(object? obj) => obj is HWND other && Equals(other);

			/// <inheritdoc />
			public override int GetHashCode() => Value.GetHashCode();

			/// <inheritdoc />
			public override string ToString() => $"0x{Value:X8}";
		}
	}
}
