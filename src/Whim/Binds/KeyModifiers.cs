using System;

namespace Whim.Core;

[Flags]
public enum KeyModifiers
{
	None = 0,

	LControl = 1,
	RControl = 2,

	LShift = 4,
	RShift = 8,

	LAlt = 16,
	RAlt = 32,

	LWin = 64,
	RWin = 128,
}
