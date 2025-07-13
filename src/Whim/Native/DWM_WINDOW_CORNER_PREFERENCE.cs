namespace Windows.Win32
{
	namespace Graphics.Dwm
	{
		/// <summary>
		/// Flags used by <see cref="PInvoke.DwmSetWindowAttribute"/> to specify the rounded
		/// corner preference for a window.
		///
		/// For more, see https://docs.microsoft.com/en-au/windows/win32/api/dwmapi/ne-dwmapi-dwm_window_corner_preference
		/// </summary>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Naming",
			"CA1707:Identifiers should not contain underscores",
			Justification = "Whim doesn't own this namespace."
		)]
		public enum DWM_WINDOW_CORNER_PREFERENCE
		{
			/// <summary>
			/// Let the system decide when to round window corners.
			/// </summary>
			DWMWCP_DEFAULT = 0,

			/// <summary>
			/// Never round window corners.
			/// </summary>
			DWMWCP_DONOTROUND = 1,

			/// <summary>
			/// Round the corners, if appropriate.
			/// </summary>
			DWMWCP_ROUND = 2,

			/// <summary>
			/// Round the corners, if appropriate, with a small radius.
			/// </summary>
			DWMWCP_ROUNDSMALL = 3,
		}
	}
}
