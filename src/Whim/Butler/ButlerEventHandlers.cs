using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class ButlerEventHandlers : IButlerEventHandlers
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly IButlerPantry _pantry;
	private readonly IButlerChores _chores;

	private int _monitorsChangingTasks;
	public bool AreMonitorsChanging => _monitorsChangingTasks > 0;

	public int MonitorsChangedDelay { init; get; } = 3 * 1000;

	public ButlerEventHandlers(
		IContext context,
		IInternalContext internalContext,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		_context = context;
		_internalContext = internalContext;
		_pantry = pantry;
		_chores = chores;
	}
}
