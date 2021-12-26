using System;

namespace Whim;

internal class LoggerNotInitializedException : Exception
{
	internal LoggerNotInitializedException() : base("The logger was not initialized 😢") { }
}
