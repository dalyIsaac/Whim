namespace Whim;

internal interface IKeybindHook : IDisposable
{
	void PostInitialize();
}
