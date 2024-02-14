# Styling

Whim uses XAML resources to style the appearance of certain user interfaces such as the [Bar Plugin](../plugins/bar.md). By default, Whim loads its resources from [Defaults.xaml](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Resources/Defaults.xaml). 

## Loading custom dictionaries

You can overwrite parts or all of the default resources by loading a custom resource dictionary. For example, the following will load `.whim/Resources.xaml` and merge it with the default dictionary.

```csharp
// Load resources in "Resources.xaml"
string file = context.FileManager.GetWhimFileDir("Resources.xaml");
context.ResourceManager.AddUserDictionary(file);
```

There is no need to provide a "complete" dictionary. Any `x:key` that isn't specified in the user dictionary, falls back to its default value.

## Resource keys

Currently, the following `x:key`s can be set by the user. A template for creating custom styles from scratch can be found [here](https://github.com/urob/whim-config/blob/main/bar.resources.xaml).

### Style keys

These keys can be used to apply arbitrary styles to certain elements such as the root panel of the status bar.

```xml
<Style x:Key="bar:root_panel" TargetType="RelativePanel" />             <!-- Root panel -->
<Style x:Key="bar:left_panel" TargetType="StackPanel" />                <!-- Left sub panel -->
<Style x:Key="bar:center_panel" TargetType="StackPanel" />              <!-- Center sub panel -->
<Style x:Key="bar:right_panel" TargetType="StackPanel" />               <!-- Right sub panel -->
<Style x:Key="bar:focused_window:text_block" TargetType="TextBlock" />  <!-- Focused window widget -->
<Style x:Key="bar:date_time:text_block" TargetType="TextBlock" />       <!-- Date-time widget -->
<Style x:Key="bar:active_layout:button" TargetType="Button" />          <!-- Active layout widget -->
<Style x:Key="bar:tree_layout:button" TargetType="Button" />            <!-- Tree layout widget -->
<Style x:Key="bar:workspace:button" TargetType="Button" />              <!-- Workspace widget -->
<Style x:Key="bar.battery.stack_panel" TargetType="StackPanel" >        <!-- StackPanel wrapper around the battery widget -->
<Style x:Key="bar:battery:font_icon" TargetType="FontIcon" />           <!-- The font icon to use for the battery widget !-->
<Style x:Key="bar:battery:text_block" TargetType="TextBlock" />         <!-- The text block for the battery widget !-->
```

For example, the following resource file defines a custom `background` color of the bar and reduces its `margins` and `padding`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Style x:Key="bar:root_panel" TargetType="RelativePanel">
        <Setter Property="Background" Value="#2e3440" />
        <Setter Property="Margin" Value="0" />
        <Setter Property="Padding" Value="2" />
    </Style>

</ResourceDictionary>
```

### Bar height

The `bar:height` key is a special key that is used to communicate to Whim the desired height of the status bar. For instance the following sets the bar height to 28.

```xml
<Style x:Key="bar:height" TargetType="RelativePanel">
    <Setter Property="Height" Value="28" />
</Style>
```
Notes:
- The corresponding style must not contain any property other than `Height`.
- This setting is overwritten if `Height` is explicitly set in <xref:Whim.Bar.BarConfig>.
- The actual height of the bar may differ from the specified one due to overflowing elements.

> [!NOTE]
> The scaling of the bar and its child elements depends on various Windows settings, which can lead to overflows and cropping.
> 
> For example, the font size is scaled by Window's "Accessibility / Text size" setting, which may change the appearance of the bar - see [here](https://github.com/dalyIsaac/Whim/issues/730#issuecomment-1863761492).

### Special color keys

XAML in WinUI does not support "trigger styles" in a user dictionary. Thus, there is no direct way for us to define styling for scenarios like button `hover` or `disabled`. As a workaround, Whim defines the following color keys which can be used to overwrite the system resources in `Microsoft.UI.Xaml.Controls`.

```xml
<Color x:Key="bar:active_workspace:background" />                       <!-- Active workspace background color -->
<Color x:Key="bar:active_workspace:foreground" />                       <!-- Active workspace foreground color -->
<Color x:Key="bar:hover:background" />                                  <!-- Mouse-over button background color -->
<Color x:Key="bar:hover:foreground" />                                  <!-- Mouse-over button foreground color -->
```
