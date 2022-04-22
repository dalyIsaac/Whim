using System.IO;

namespace Whim;

public static class LoggerStringExtensions
{
	public static string AddCaller(this string message, string memberName, string sourceFilePath, int sourceLineNumber)
	{
		string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		string fileLocation = $"{fileName}:{sourceLineNumber}";

		string methodName = $"[{memberName}]";

		return $"{fileLocation} {methodName} {message}";
	}
}
