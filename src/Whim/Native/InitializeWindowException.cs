namespace Whim;

[System.Serializable]
public class InitializeWindowException : System.Exception
{
	public InitializeWindowException() { }
	public InitializeWindowException(string message) : base(message) { }
	public InitializeWindowException(string message, System.Exception inner) : base(message, inner) { }
	protected InitializeWindowException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
