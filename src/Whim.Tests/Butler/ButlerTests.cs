using System;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ButlerTests
{
	#region SetPantry
	[Theory, AutoSubstituteData]
	[SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing the setter")]
	internal void SetPantry_BeforeInitialize(IContext ctx, IInternalContext internalContext, IButlerPantry pantry)
	{
		// Given the pantry is not set
		Butler sut = new(ctx, internalContext);

		// When we attempt to set the pantry
		sut.Pantry = pantry;

		// Then the pantry is set
		Assert.Equal(pantry, sut.Pantry);
	}

	[Theory, AutoSubstituteData]
	internal void SetPantry_AfterInitialize(IContext ctx, IInternalContext internalContext, IButlerPantry pantry)
	{
		// Given the pantry is not set
		Butler sut = new(ctx, internalContext);
		sut.Initialize();

		// When we attempt to set the pantry
		sut.Pantry = pantry;

		// Then the pantry is not set
		Assert.NotEqual(pantry, sut.Pantry);
	}
	#endregion

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalContext)
	{
		// Given the butler is initialized
		Butler sut = new(ctx, internalContext);
		sut.Initialize();

		// When we dispose the butler
		sut.Dispose();

		// Then the event handlers are disposed
		ctx.WindowManager.WindowAdded -= Arg.Any<EventHandler<WindowEventArgs>>();
	}
}
