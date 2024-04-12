namespace Whim;

/// <summary>
/// Data specifying how to update the <see cref="ISlice{TState}"/>. Analagous to a Redux action.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
/// <param name="Key">The unique string key specifying which update to perform.</param>
public record Transform(string Key);

///// <summary>
///// A "slice" or a specific portion of the overall state.
///// </summary>
///// <typeparam name="TState"></typeparam>
//public interface ISlice<TState>
//{
//	/// <summary>
//	/// Dispatch a <see cref="Transform"/> to update the <see cref="IStoreData"/> of the <see cref="Store"/>
//	/// </summary>
//	/// <typeparam name="T">The payload of the <see cref="Transform"/></typeparam>
//	/// <param name="transform">The <see cref="Transform"/> to update the state.</param>
//	void Dispatch<T>(Transform transform);

//	/// <summary>
//	/// Extract state from the <typeparamref name="TState"/>. Analagous to a Redux selector.
//	/// </summary>
//	/// <typeparam name="TResult"></typeparam>
//	/// <param name="picker"></param>
//	/// <returns></returns>
//	TResult Pick<TResult>(Func<TState, TResult> picker);
//}

public interface ISlice
{
	/// <summary>
	/// Dispatch a <see cref="Transform"/> to update the <see cref="IStoreData"/> of the <see cref="Store"/>
	/// </summary>
	/// <typeparam name="T">The payload of the <see cref="Transform"/></typeparam>
	/// <param name="transform">The <see cref="Transform"/> to update the state.</param>
	void Dispatch<T>(Transform transform);
}
