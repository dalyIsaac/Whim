using System;

namespace Whim.Core;

internal class LoggerUninitializedException : Exception
{
	internal LoggerUninitializedException() : base("The logger was not initialized 😢") { }
}
