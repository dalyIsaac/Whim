namespace Whim;

public interface IPlugin
{
	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin after the rest of the <see cref="IConfigContext"/> has been initialized.
	/// </summary>
	public void Initialize();
}
