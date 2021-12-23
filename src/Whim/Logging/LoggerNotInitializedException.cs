using System;

namespace Whim.Core;

internal class LoggerNotInitializedException : Exception
{
	internal LoggerNotInitializedException() : base("The logger was not initialized 😢") { }
}
