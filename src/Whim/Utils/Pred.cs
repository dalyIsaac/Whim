namespace Whim;

/// <summary>
/// A generic predicate.
/// </summary>
/// <typeparam name="T">
/// The type of the object.
/// </typeparam>
/// <param name="obj">
/// The object.
/// </param>
/// <param name="idx">
/// The index of the object.
/// </param>
/// <param name="ctx">
/// The context.
/// </param>
/// <returns>
/// Whether the object should be considered.
/// </returns>
public delegate bool Pred<in T>(T obj, int idx, IContext ctx);

/// <summary>
/// Extensions for <see cref="Pred{T}"/>
/// </summary>
public static class PredExtensions
{
	/// <summary>
	/// Gets the index of the first object in the enumerable that matches the predicate.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="enumerable"></param>
	/// <param name="pred"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static int GetMatchingIndex<T>(this IEnumerable<T> enumerable, Pred<T> pred, IContext ctx)
	{
		int idx = 0;

		foreach (T obj in enumerable)
		{
			if (pred(obj, idx, ctx))
			{
				return idx;
			}

			idx++;
		}

		return -1;
	}
}
