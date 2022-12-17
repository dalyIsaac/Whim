using System;
using System.Diagnostics;

namespace Windows.Win32
{
	namespace UI.WindowsAndMessaging
	{
		/// <summary>
		/// A handle to a multiple-window position structure.
		/// </summary>
		[DebuggerDisplay("{Value}")]
		public readonly struct HDWP : IEquatable<HDWP>
		{
			internal readonly nint Value;

			/// <summary>
			/// Creates a new <see cref="HDWP"/> from the given <paramref name="value"/>.
			/// </summary>
			/// <param name="value"></param>
			internal HDWP(nint value) => Value = value;

			/// <inheritdoc/>
			public static implicit operator nint(HDWP value) => value.Value;

			/// <inheritdoc/>
			public static explicit operator HDWP(nint value) => new(value);

			/// <inheritdoc/>
			public static bool operator ==(HDWP left, HDWP right) => left.Value == right.Value;

			/// <inheritdoc/>
			public static bool operator !=(HDWP left, HDWP right) => !(left == right);

			/// <inheritdoc/>
			public bool Equals(HDWP other) => Value == other.Value;

			/// <inheritdoc/>
			public override bool Equals(object? obj) => obj is HDWP other && Equals(other);

			/// <inheritdoc/>
			public override int GetHashCode() => Value.GetHashCode();

			/// <inheritdoc/>
			public override string ToString() => Value.ToString();
		}
	}
}
