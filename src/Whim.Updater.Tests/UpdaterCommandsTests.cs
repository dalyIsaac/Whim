using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Updater.Tests;

public class UpdaterCommandsCustomization : ICustomization
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	public void Customize(IFixture fixture)
	{
		IUpdaterPlugin plugin = fixture.Create<IUpdaterPlugin>();
		plugin.Name.Returns("whim.updater");
		fixture.Inject(plugin);
	}
}

public class UpdaterCommandsTests
{
	[InlineAutoSubstituteData<UpdaterCommandsCustomization>("whim.updater.check")]
	[Theory]
	public void CheckForUpdates(string commandId, IUpdaterPlugin plugin)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(new UpdaterCommands(plugin)).GetCommand(commandId);

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).CheckForUpdates();
	}
}
