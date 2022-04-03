using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace Whim.App;

internal class Whim
{
	internal static void Start()
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

		// Create an empty Whim config context.
		IConfigContext configContext = new ConfigContext();
		configContext = doConfig(configContext);

		// Initialize Whim.
		configContext.Initialize();

		// Start the application and the message loop.
		Logger.Information("Starting application...");

		global::Microsoft.UI.Xaml.Application.Start((p) =>
			{
				var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
				global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
				new App(configContext);
			});
	}
}
