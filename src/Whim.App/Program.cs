using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim.App;

#if DISABLE_XAML_GENERATED_MAIN
public static class Program
{
	[global::System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
	private static extern void XamlCheckProcessRequirements();

	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 1.0.0.0")]
	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
	[global::System.STAThreadAttribute]
	// Replaces the standard App.g.i.cs.
	// Note: We can't declare Main to be async because in a WinUI app
	// that prevents Narrator from reading XAML elements.
	static void Main(string[] args)
	{
		XamlCheckProcessRequirements();

		global::WinRT.ComWrappersSupport.InitializeComWrappers();

		bool this_is_the_first_instance = true;

		// If this is the first instance launched, then register it as the "main" instance.
		// If this isn't the first instance launched, then "main" will already be registered,
		// so retrieve it.
		var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");

		// If the instance that's executing the OnLaunched handler right now
		// isn't the "main" instance.
		if (!mainInstance.IsCurrent)
		{
			this_is_the_first_instance = false;

			// Redirect the activation (and args) to the "main" instance, and exit.
			var activatedEventArgs =
				Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
			mainInstance.RedirectActivationToAsync(activatedEventArgs);
		}

		if (this_is_the_first_instance)
		{
			global::Microsoft.UI.Xaml.Application.Start((p) =>
			{
				var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
				global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
				new App(Config.CreateConfigContext());
			});
		}
	}
}
#endif
