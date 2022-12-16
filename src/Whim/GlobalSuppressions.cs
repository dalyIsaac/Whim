// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Naming",
	"CA1710:Identifiers should have correct suffix",
	Justification = "ILayoutEngine's primary purpose is not as a collection",
	Scope = "type",
	Target = "~T:Whim.ILayoutEngine"
)]
[assembly: SuppressMessage(
	"Maintainability",
	"CA1508:Avoid dead conditional code",
	Justification = "Check is required for multiple threads",
	Scope = "member",
	Target = "~P:Whim.Screen.DesktopChangedCount"
)]
[assembly: SuppressMessage(
	"Design",
	"CA1000:Do not declare static members on generic types",
	Justification = "Will be resolved by #51",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Design",
	"CA1051:Do not declare visible instance fields",
	Justification = "They are used by subclasses",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Design",
	"CA1002:Do not expose generic lists",
	Justification = "They are used by subclasses",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Naming",
	"CA1716:Identifiers should not match keywords",
	Justification = "Not concerned about Visual Basic",
	Scope = "member",
	Target = "~M:Whim.IConfigContext.Exit(Whim.ExitEventArgs)"
)]
[assembly: SuppressMessage(
	"Style",
	"IDE0021:Use block body for constructors",
	Justification = "It should be allowed for limited cases",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Style",
	"IDE0023:Use block body for conversion operators",
	Justification = "It should be allowed for limited cases",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Style",
	"IDE0024:Use block body for operators",
	Justification = "It should be allowed for limited cases",
	Scope = "module"
)]
[assembly: SuppressMessage(
	"Usage",
	"CA2225:Operator overloads have named alternates",
	Justification = "Superfluous for this case",
	Scope = "member",
	Target = "~M:Windows.Win32.Foundation.HWND.op_Explicit(System.IntPtr)~Windows.Win32.Foundation.HWND"
)]
[assembly: SuppressMessage(
	"Usage",
	"CA2225:Operator overloads have named alternates",
	Justification = "Superfluous for this case",
	Scope = "member",
	Target = "~M:Windows.Win32.UI.WindowsAndMessaging.HDWP.op_Explicit(System.IntPtr)~Windows.Win32.UI.WindowsAndMessaging.HDWP"
)]
