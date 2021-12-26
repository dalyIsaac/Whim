using System;

namespace Whim.Bar;
public class BarPlugin : IPlugin
{
	private readonly IConfigContext _configContext;

	public BarPlugin(IConfigContext configContext)
	{
		_configContext = configContext;
		// TODO: subscribe to the monitor change event
	}
}
