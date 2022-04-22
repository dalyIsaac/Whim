using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
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
				DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
				SynchronizationContext.SetSynchronizationContext(context);
				_ = new App(configContext, startupException);
			});
	}

	/// <summary>
	/// Acquires and evaluates the user's <see cref="IConfigContext"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <returns></returns>
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
