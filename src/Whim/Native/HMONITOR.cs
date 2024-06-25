using System.Diagnostics;

namespace Windows.Win32
{
	namespace Graphics.Gdi
	{
		/// <summary>
		/// A handle to a monitor.
		/// </summary>
		[DebuggerDisplay("{Value}")]
		public readonly struct HMONITOR : IEquatable<HMONITOR>
		{
			internal readonly nint Value;

			internal HMONITOR(nint value) => Value = value;

			/// <inheritdoc />
			public static implicit operator nint(HMONITOR value) => value.Value;

			/// <inheritdoc />
			public static explicit operator HMONITOR(nint value) => new(value);

			/// <inheritdoc />
			public static bool operator ==(HMONITOR left, HMONITOR right) => left.Value == right.Value;

			/// <inheritdoc />
			public static bool operator !=(HMONITOR left, HMONITOR right) => !(left == right);

			/// <inheritdoc />
			public bool Equals(HMONITOR other) => Value == other.Value;

			/// <inheritdoc />
			public override bool Equals(object? obj) => obj is HMONITOR other && Equals(other);

			/// <inheritdoc />
			public override int GetHashCode() => Value.GetHashCode();

			/// <inheritdoc />
			public override string ToString() => $"0x{Value:X8}";
		}
	}
}
