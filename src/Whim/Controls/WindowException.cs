namespace Whim;

[System.Serializable]
public class InitializeBorderlessWindowException : System.Exception
{
	public InitializeBorderlessWindowException() { }
	public InitializeBorderlessWindowException(string message) : base(message) { }
	public InitializeBorderlessWindowException(string message, System.Exception inner) : base(message, inner) { }
	protected InitializeBorderlessWindowException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
