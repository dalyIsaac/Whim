using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace Whim.Runner;

internal class Whim
{
	internal static void Start()
	{
		// Create an empty Whim config context.
		IConfigContext configContext = new ConfigContext();
		Exception? startupException = null;

		try
		{
			configContext = GetConfigContext(configContext);
		}
		catch (Exception ex)
		{
			startupException = ex;
		}

		// Start the application and the message loop.
		global::Microsoft.UI.Xaml.Application.Start((p) =>
			{
				var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
				global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
				new App(configContext, startupException);
			});
	}

	private static IConfigContext GetConfigContext(IConfigContext configContext)
	{
		// Ensure the Whim directory exists.
		FileHelper.EnsureWhimDirExists();

		// Acquire the Whim config.
		bool configExists = ConfigHelper.DoesConfigExist();
		if (!configExists)
		{
			ConfigHelper.CreateConfig();
		}

		string rawConfig = ConfigHelper.LoadConfig();

		// Evaluate the Whim config.
		ScriptOptions options = ScriptOptions.Default;
		Task<Func<IConfigContext, IConfigContext>> task = CSharpScript.EvaluateAsync<Func<IConfigContext, IConfigContext>>(rawConfig, options);
		Func<IConfigContext, IConfigContext> doConfig = task.Result;

		return doConfig(configContext);
	}
}
