using System.Diagnostics;

namespace Windows.Win32
{
	namespace UI.WindowsAndMessaging
	{
		/// <summary>
		/// A handle to a multiple-window position structure.
		/// </summary>
		[DebuggerDisplay("{Value}")]
		public readonly record struct HDWP
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
		}
	}
}
