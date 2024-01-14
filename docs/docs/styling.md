# Styling

Suppose the user wants to change the `background` color of the bar and reduce the `margins` and `padding`. They can do so by saving the following code as `Resources.xaml` to their local `.whim` config directory:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Style x:Key="bar:root_panel" TargetType="RelativePanel">
        <Setter Property="Background" Value="#2e3440" />
        <Setter Property="Margin" Value="0" />
        <Setter Property="Padding" Value="2" />
    </Style>

</ResourceDictionary>
```

There is no need to provide a complete style sheet. All `x:key`s that aren't set in the users style sheet fall back to their default values as set in the packaged `/Resources/Defaults.xaml`. A more elaborate style sheet that can be used as template can be [downloaded here](https://github.com/urob/whim-config/blob/main/bar.resources.xaml).

The user can load their local style sheet by adding the following lines to their `whim.config.csx`.

```csharp
// ...

void DoConfig(IContext context)
{
    // overload bar defaults with local "Resources.xaml"
    string file = context.FileManager.GetWhimFileDir("Resources.xaml");
    context.ResourceManager.SetUserDictionary(file);
}
return DoConfig;
```

## Resource keys

Currently, the following `x:key`s can be set by the user:

```xml
<Style x:Key="bar:height" TargetType="RelativePanel" />                 <!-- Height of the bar -->
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
<Color x:Key="bar:active_workspace:background" />                       <!-- Active workspace background color -->
<Color x:Key="bar:active_workspace:foreground" />                       <!-- Active workspace foreground color -->
<Color x:Key="bar:hover:background" />                                  <!-- Mouse-over button background color -->
<Color x:Key="bar:hover:foreground" />                                  <!-- Mouse-over button foreground color -->

```
