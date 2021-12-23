namespace Whim.Core;

public interface IPlugin
{
	/// <summary>
	/// Initializes the plugin after the rest of the <see cref="IConfigContext"/> has been initialized.
	/// </summary>
	public void Initialize();
}
