# Gap Plugin

The <xref:Whim.Gaps.GapsPlugin> is a plugin which inserts a <xref:Whim.Gaps.GapsLayoutEngine> proxy layout engine to add gaps between each of the windows in the layout.

![Gaps plugin demo](../../images/gaps-demo.png)

This behavior can be customized with the <xref:Whim.Gaps.GapsConfig> provided to the <Whim.Gaps.GapsCommands.#ctor(Whim.Gaps.IGapsPlugin)> and the following commands in <xref:Whim.Gaps.GapsCommands>:

| Identifier                 | Title              | Keybind                                                            |
| -------------------------- | ------------------ | ------------------------------------------------------------------ |
| `whim.gaps.outer.increase` | Increase outer gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>L</kbd> |
| `whim.gaps.outer.decrease` | Decrease outer gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>H</kbd> |
| `whim.gaps.inner.increase` | Increase inner gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> |
| `whim.gaps.inner.decrease` | Decrease inner gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>J</kbd> |
