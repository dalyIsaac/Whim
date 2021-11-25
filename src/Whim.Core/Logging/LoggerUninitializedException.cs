using System;

namespace Whim.Core.Logging;

internal class LoggerUninitializedException : Exception
{
	internal LoggerUninitializedException() : base("The logger was not initialized 😢") { }
}
