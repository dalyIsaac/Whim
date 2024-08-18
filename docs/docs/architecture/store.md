# Store

The <xref:Whim.IStore> contains the majority of Whim's state. It is split into `Sector`s and exposed via two interfaces:

- <xref href="Whim.IStore.Dispatch``1(Whim.Transform{``0})" /> to dispatch <xref:Whim.Transform>s to the store
- <xref href="Whim.IStore.Pick``1(Whim.Picker{``0})" /> to "pick" values from the store

## `Transform`

A <xref:Whim.Transform`1> is an operation describing how to update the state of the <xref:Whim.IStore>. Transforms are overrides of the abstract record <xref:Whim.Transform`1>, and the `Execute` method takes in a mutable reference to the store. Transforms will always return a `Result<T>` - for more, see the [Result](./result.md) page.

## `Picker`

A "picker" is an operation describing how to retrieve a value from the store. Pickers are usually a function implementing the delegate <xref:Whim.PurePicker`1>, but can be stateful subclasses of <xref:Whim.Picker`1>.

Pickers take in an immutable reference to the store and return a value. If the picking operation may fail, the picker should return a [`Result<T>`](./result.md).

## Sectors and Immutability

The store contains the <xref:Whim.IRootSector>, which in turn contains each sector. A "sector" is a collection of related state in the store. For example, the <xref:Whim.IMonitorSector> contains all the monitors in the system.

The `public` interface of each sector presents an immutable view of the sector's state. This is to prevent the store from being mutated outside of a transform. The `internal` interface of each sector is mutable and is used by transforms to update the sector's state.

### Events

Each sector typically has a related event listener - for example, the <xref:Whim.IWindowSector> uses the `WindowEventListener` to listen to window events from Windows. For more, see the [Events](./events.md) page.

Transforms _queue_ events to be executed after the original transform in the thread is executed (see [Threading](#threading)). This is done by calling `QueueEvent` on the appropriate sector for the event.

Events can be subscribed to from the `Events` properties on the <xref:Whim.IStore>. For example:

```csharp
public void Subscribe()
{
    context.Store.WindowEvents.WindowMoved += WindowMoved;
}

private void WindowMoved(object? sender, WindowMoveEventArgs e)
{
    // Do something
}
```

### `WorkspaceSector`

The `WorkspaceSector` does a few unique things. After every `Transform` is executed but before the events are dispatched, the `WorkspaceSector` will check to see if any "layout" operations have been queued for workspaces. If so, it will execute them. Afterwards, it will then focus the single `WindowHandleToFocus`.

Transforms will queue layouts internally by calling `DoWorkspaceLayoutTransform`. Similarly, focusing a window can be done by calling the <xref:Whim.FocusWindowTransform>.

## Threading

Whim is a multi-threaded application. The main thread is a [Single-Threaded Apartment (STA)](https://learn.microsoft.com/en-us/windows/win32/com/single-threaded-apartments), started by the Windows App SDK.

STA applications allow the main thread to have events from Windows call a new sequence of execution while waiting for a time-consuming operation (e.g., a Win32 API call). This is called [reentrancy](<https://en.wikipedia.org/wiki/Reentrancy_(computing)>). Whim, being a Windows application which performs a large number of Win32 API calls, was particularly susceptible to this.

To prevent reentrancy, whenever Whim receives an event from Windows, it will dispatch a transform to the store. Dispatching will block the main thread until the transform has been executed in a separate thread. To prevent concurrency issues, the store is locked with a [`ReaderWriterLockSlim`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim). Dispatching transforms will use a write lock, while picking values will use a read lock.

If a `Transform A` calls a `Transform B`, the store will execute the `Transform B` in the same thread as `Transform A`.
