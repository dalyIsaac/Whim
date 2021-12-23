using System.IO;

namespace Whim.Core;

public static class LoggerStringExtensions
{
	public static string AddCaller(this string message, string memberName, string sourceFilePath, int sourceLineNumber){
		string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		string fileLocation = $"{fileName}:{sourceLineNumber}".PadRight(30);

		string methodName = $"[{memberName}]".PadRight(30);

		return $"{fileLocation} {methodName} {message}";
	}
}
