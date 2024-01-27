# Threading

Whim operates on a single thread and uses the [Single-Threaded Apartments (STAs)](https://learn.microsoft.com/en-us/windows/win32/com/single-threaded-apartments) model.

Unfortunately, Whim is subject to reentrancy as part of using STAs. For a good overview of reentrancy, see [What is so special about the Application STA?](https://devblogs.microsoft.com/oldnewthing/20210224-00/?p=104901) by Raymond Chen's The New Old Thing blog.

STA reentrancy has caused issues where Whim would:

1. Enter and do some processing
2. While waiting for something, enter again due to another event, and set the window positions
3. Go back to the deferred block of execution and set the window positions

This could cause loops where a sort of infinite loop would be entered, as windows would continually move between different positions ([dalyIsaac/Whim#446](https://github.com/dalyIsaac/Whim/issues/446)).

As it is "[not possible to prevent reentrancy](https://learn.microsoft.com/en-us/windows/win32/winauto/guarding-against-reentrancy-in-hook-functions)", Whim has a `DeferWindowPosManager` which checks for reentrancy in the current stack. If reentrancy has been detected, `DeferWindowPosHandle` has its layout deferred until there are no reentrant execution blocks in the stack. This was implemented in [dalyIsaac/Whim#553](https://github.com/dalyIsaac/Whim/pull/553).

On a similar vein, `Workspace` has a method `GarbageCollect` which attempts to remove windows from the workspace which are no longer valid.
