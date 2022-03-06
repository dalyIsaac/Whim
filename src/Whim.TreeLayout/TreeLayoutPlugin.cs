namespace Whim.TreeLayout;

public class TreeLayoutPlugin : IPlugin
{
	private readonly IConfigContext _configContext;

	public TreeLayoutPlugin(IConfigContext configContext)
	{
		_configContext = configContext;
		_configContext.FilterManager.IgnoreTitleMatch("Whim TreeLayout Phantom Window");
	}

	public void Initialize() { }
}
