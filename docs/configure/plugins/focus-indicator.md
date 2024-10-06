# Focus Indicator Plugin

The `FocusIndicatorPlugin` adds a focus indicator to the focused window.

![Focus indicator demo](../../images/focus-indicator-demo.gif)

```yaml
plugins:
  focus_indicator:
    is_enabled: true
    border_size: 10
    is_fade_enabled: true
    fade_timeout: 2
    color: green
```

## Configuration

| Property          | Description                                                                                                                  |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `is_enabled`      | Whether the plugin is enabled. Defaults to `true`.                                                                           |
| `color`           | The color of the focus indicator. Default is `red`.                                                                          |
| `border_size`     | The size of the border of the focus indicator, in pixels. Default is `4`.                                                    |
| `is_fade_enabled` | When `true`, the focus indicator will be visible for `fade_timeout` seconds after the window is focused. Default is `false`. |
| `fade_timeout`    | The time in seconds before the focus indicator fades out. Default is `10`.                                                   |

## Colors

The color can be any valid RGB or RGBA hex color. The following named colors are also supported:

| Color Name             | RGBA Value  | Preview                                                              |
| ---------------------- | ----------- | -------------------------------------------------------------------- |
| `alice_blue`           | `#FFF0F8FF` | <span style="background:#F0F8FF" class="color-block"></span>         |
| `antique_white`        | `#FFFAEBD7` | <span style="background:#FAEBD7" class="color-block"></span>         |
| `aqua`                 | `#FF00FFFF` | <span style="background:#00FFFF" class="color-block"></span>         |
| `aquamarine`           | `#FF7FFFD4` | <span style="background:#7FFFD4" class="color-block"></span>         |
| `azure`                | `#FFF0FFFF` | <span style="background:#F0FFFF" class="color-block"></span>         |
| `beige`                | `#FFF5F5DC` | <span style="background:#F5F5DC" class="color-block"></span>         |
| `bisque`               | `#FFFFE4C4` | <span style="background:#FFE4C4" class="color-block"></span>         |
| `black`                | `#FF000000` | <span style="background:#000000" class="color-block"></span>         |
| `blanched_almond`      | `#FFFFEBCD` | <span style="background:#FFEBCD" class="color-block"></span>         |
| `blue`                 | `#FF0000FF` | <span style="background:#0000FF" class="color-block"></span>         |
| `blue_violet`          | `#FF8A2BE2` | <span style="background:#8A2BE2" class="color-block"></span>         |
| `brown`                | `#FFA52A2A` | <span style="background:#A52A2A" class="color-block"></span>         |
| `burly_wood`           | `#FFDEB887` | <span style="background:#DEB887" class="color-block"></span>         |
| `cadet_blue`           | `#FF5F9EA0` | <span style="background:#5F9EA0" class="color-block"></span>         |
| `chartreuse`           | `#FF7FFF00` | <span style="background:#7FFF00" class="color-block"></span>         |
| `chocolate`            | `#FFD2691E` | <span style="background:#D2691E" class="color-block"></span>         |
| `coral`                | `#FFFF7F50` | <span style="background:#FF7F50" class="color-block"></span>         |
| `cornflower_blue`      | `#FF6495ED` | <span style="background:#6495ED" class="color-block"></span>         |
| `cornsilk`             | `#FFFFF8DC` | <span style="background:#FFF8DC" class="color-block"></span>         |
| `crimson`              | `#FFDC143C` | <span style="background:#DC143C" class="color-block"></span>         |
| `cyan`                 | `#FF00FFFF` | <span style="background:#00FFFF" class="color-block"></span>         |
| `dark_blue`            | `#FF00008B` | <span style="background:#00008B" class="color-block"></span>         |
| `dark_cyan`            | `#FF008B8B` | <span style="background:#008B8B" class="color-block"></span>         |
| `dark_goldenrod`       | `#FFB8860B` | <span style="background:#B8860B" class="color-block"></span>         |
| `dark_gray`            | `#FFA9A9A9` | <span style="background:#A9A9A9" class="color-block"></span>         |
| `dark_green`           | `#FF006400` | <span style="background:#006400" class="color-block"></span>         |
| `dark_khaki`           | `#FFBDB76B` | <span style="background:#BDB76B" class="color-block"></span>         |
| `dark_magenta`         | `#FF8B008B` | <span style="background:#8B008B" class="color-block"></span>         |
| `dark_olive_green`     | `#FF556B2F` | <span style="background:#556B2F" class="color-block"></span>         |
| `dark_orange`          | `#FFFF8C00` | <span style="background:#FF8C00" class="color-block"></span>         |
| `dark_orchid`          | `#FF9932CC` | <span style="background:#9932CC" class="color-block"></span>         |
| `dark_red`             | `#FF8B0000` | <span style="background:#8B0000" class="color-block"></span>         |
| `dark_salmon`          | `#FFE9967A` | <span style="background:#E9967A" class="color-block"></span>         |
| `dark_sea_green`       | `#FF8FBC8F` | <span style="background:#8FBC8F" class="color-block"></span>         |
| `dark_slate_blue`      | `#FF483D8B` | <span style="background:#483D8B" class="color-block"></span>         |
| `dark_slate_gray`      | `#FF2F4F4F` | <span style="background:#2F4F4F" class="color-block"></span>         |
| `dark_turquoise`       | `#FF00CED1` | <span style="background:#00CED1" class="color-block"></span>         |
| `dark_violet`          | `#FF9400D3` | <span style="background:#9400D3" class="color-block"></span>         |
| `deep_pink`            | `#FFFF1493` | <span style="background:#FF1493" class="color-block"></span>         |
| `deep_sky_blue`        | `#FF00BFFF` | <span style="background:#00BFFF" class="color-block"></span>         |
| `dim_gray`             | `#FF696969` | <span style="background:#696969" class="color-block"></span>         |
| `dodger_blue`          | `#FF1E90FF` | <span style="background:#1E90FF" class="color-block"></span>         |
| `firebrick`            | `#FFB22222` | <span style="background:#B22222" class="color-block"></span>         |
| `floral_white`         | `#FFFFFAF0` | <span style="background:#FFFAF0" class="color-block"></span>         |
| `forest_green`         | `#FF228B22` | <span style="background:#228B22" class="color-block"></span>         |
| `fuchsia`              | `#FFFF00FF` | <span style="background:#FF00FF" class="color-block"></span>         |
| `gainsboro`            | `#FFDCDCDC` | <span style="background:#DCDCDC" class="color-block"></span>         |
| `ghost_white`          | `#FFF8F8FF` | <span style="background:#F8F8FF" class="color-block"></span>         |
| `gold`                 | `#FFFFD700` | <span style="background:#FFD700" class="color-block"></span>         |
| `goldenrod`            | `#FFDAA520` | <span style="background:#DAA520" class="color-block"></span>         |
| `gray`                 | `#FF808080` | <span style="background:#808080" class="color-block"></span>         |
| `green`                | `#FF008000` | <span style="background:#008000" class="color-block"></span>         |
| `green_yellow`         | `#FFADFF2F` | <span style="background:#ADFF2F" class="color-block"></span>         |
| `honeydew`             | `#FFF0FFF0` | <span style="background:#F0FFF0" class="color-block"></span>         |
| `hot_pink`             | `#FFFF69B4` | <span style="background:#FF69B4" class="color-block"></span>         |
| `indian_red`           | `#FFCD5C5C` | <span style="background:#CD5C5C" class="color-block"></span>         |
| `indigo`               | `#FF4B0082` | <span style="background:#4B0082" class="color-block"></span>         |
| `ivory`                | `#FFFFFFF0` | <span style="background:#FFFFF0" class="color-block"></span>         |
| `khaki`                | `#FFF0E68C` | <span style="background:#F0E68C" class="color-block"></span>         |
| `lavender`             | `#FFE6E6FA` | <span style="background:#E6E6FA" class="color-block"></span>         |
| `lavender_blush`       | `#FFFFF0F5` | <span style="background:#FFF0F5" class="color-block"></span>         |
| `lawn_green`           | `#FF7CFC00` | <span style="background:#7CFC00" class="color-block"></span>         |
| `lemon_chiffon`        | `#FFFFFACD` | <span style="background:#FFFACD" class="color-block"></span>         |
| `light_blue`           | `#FFADD8E6` | <span style="background:#ADD8E6" class="color-block"></span>         |
| `light_coral`          | `#FFF08080` | <span style="background:#F08080" class="color-block"></span>         |
| `light_cyan`           | `#FFE0FFFF` | <span style="background:#E0FFFF" class="color-block"></span>         |
| `light_goldenrod_yell` | `#FFFAFAD2` | <span style="background:#FAFAD2" class="color-block"></span>         |
| `light_gray`           | `#FFD3D3D3` | <span style="background:#D3D3D3" class="color-block"></span>         |
| `light_green`          | `#FF90EE90` | <span style="background:#90EE90" class="color-block"></span>         |
| `light_pink`           | `#FFFFB6C1` | <span style="background:#FFB6C1" class="color-block"></span>         |
| `light_salmon`         | `#FFFFA07A` | <span style="background:#FFA07A" class="color-block"></span>         |
| `light_sea_green`      | `#FF20B2AA` | <span style="background:#20B2AA" class="color-block"></span>         |
| `light_sky_blue`       | `#FF87CEFA` | <span style="background:#87CEFA" class="color-block"></span>         |
| `light_slate_gray`     | `#FF778899` | <span style="background:#778899" class="color-block"></span>         |
| `light_steel_blue`     | `#FFB0C4DE` | <span style="background:#B0C4DE" class="color-block"></span>         |
| `light_yellow`         | `#FFFFFFE0` | <span style="background:#FFFFE0" class="color-block"></span>         |
| `lime`                 | `#FF00FF00` | <span style="background:#00FF00" class="color-block"></span>         |
| `lime_green`           | `#FF32CD32` | <span style="background:#32CD32" class="color-block"></span>         |
| `linen`                | `#FFFAF0E6` | <span style="background:#FAF0E6" class="color-block"></span>         |
| `magenta`              | `#FFFF00FF` | <span style="background:#FF00FF" class="color-block"></span>         |
| `maroon`               | `#FF800000` | <span style="background:#800000" class="color-block"></span>         |
| `medium_aquamarine`    | `#FF66CDAA` | <span style="background:#66CDAA" class="color-block"></span>         |
| `medium_blue`          | `#FF0000CD` | <span style="background:#0000CD" class="color-block"></span>         |
| `medium_orchid`        | `#FFBA55D3` | <span style="background:#BA55D3" class="color-block"></span>         |
| `medium_purple`        | `#FF9370DB` | <span style="background:#9370DB" class="color-block"></span>         |
| `medium_sea_green`     | `#FF3CB371` | <span style="background:#3CB371" class="color-block"></span>         |
| `medium_slate_blue`    | `#FF7B68EE` | <span style="background:#7B68EE" class="color-block"></span>         |
| `medium_spring_green`  | `#FF00FA9A` | <span style="background:#00FA9A" class="color-block"></span>         |
| `medium_turquoise`     | `#FF48D1CC` | <span style="background:#48D1CC" class="color-block"></span>         |
| `medium_violet_red`    | `#FFC71585` | <span style="background:#C71585" class="color-block"></span>         |
| `midnight_blue`        | `#FF191970` | <span style="background:#191970" class="color-block"></span>         |
| `mint_cream`           | `#FFF5FFFA` | <span style="background:#F5FFFA" class="color-block"></span>         |
| `misty_rose`           | `#FFFFE4E1` | <span style="background:#FFE4E1" class="color-block"></span>         |
| `moccasin`             | `#FFFFE4B5` | <span style="background:#FFE4B5" class="color-block"></span>         |
| `navajo_white`         | `#FFFFDEAD` | <span style="background:#FFDEAD" class="color-block"></span>         |
| `navy`                 | `#FF000080` | <span style="background:#000080" class="color-block"></span>         |
| `old_lace`             | `#FFFDF5E6` | <span style="background:#FDF5E6" class="color-block"></span>         |
| `olive`                | `#FF808000` | <span style="background:#808000" class="color-block"></span>         |
| `olive_drab`           | `#FF6B8E23` | <span style="background:#6B8E23" class="color-block"></span>         |
| `orange`               | `#FFFFA500` | <span style="background:#FFA500" class="color-block"></span>         |
| `orange_red`           | `#FFFF4500` | <span style="background:#FF4500" class="color-block"></span>         |
| `orchid`               | `#FFDA70D6` | <span style="background:#DA70D6" class="color-block"></span>         |
| `pale_goldenrod`       | `#FFEEE8AA` | <span style="background:#EEE8AA" class="color-block"></span>         |
| `pale_green`           | `#FF98FB98` | <span style="background:#98FB98" class="color-block"></span>         |
| `pale_turquoise`       | `#FFAFEEEE` | <span style="background:#AFEEEE" class="color-block"></span>         |
| `pale_violet_red`      | `#FFDB7093` | <span style="background:#DB7093" class="color-block"></span>         |
| `papaya_whip`          | `#FFFFEFD5` | <span style="background:#FFEFD5" class="color-block"></span>         |
| `peach_puff`           | `#FFFFDAB9` | <span style="background:#FFDAB9" class="color-block"></span>         |
| `peru`                 | `#FFCD853F` | <span style="background:#CD853F" class="color-block"></span>         |
| `pink`                 | `#FFFFC0CB` | <span style="background:#FFC0CB" class="color-block"></span>         |
| `plum`                 | `#FFDDA0DD` | <span style="background:#DDA0DD" class="color-block"></span>         |
| `powder_blue`          | `#FFB0E0E6` | <span style="background:#B0E0E6" class="color-block"></span>         |
| `purple`               | `#FF800080` | <span style="background:#800080" class="color-block"></span>         |
| `red`                  | `#FFFF0000` | <span style="background:#FF0000" class="color-block"></span>         |
| `rosy_brown`           | `#FFBC8F8F` | <span style="background:#BC8F8F" class="color-block"></span>         |
| `royal_blue`           | `#FF4169E1` | <span style="background:#4169E1" class="color-block"></span>         |
| `saddle_brown`         | `#FF8B4513` | <span style="background:#8B4513" class="color-block"></span>         |
| `salmon`               | `#FFFA8072` | <span style="background:#FA8072" class="color-block"></span>         |
| `sandy_brown`          | `#FFF4A460` | <span style="background:#F4A460" class="color-block"></span>         |
| `sea_green`            | `#FF2E8B57` | <span style="background:#2E8B57" class="color-block"></span>         |
| `sea_shell`            | `#FFFFF5EE` | <span style="background:#FFF5EE" class="color-block"></span>         |
| `sienna`               | `#FFA0522D` | <span style="background:#A0522D" class="color-block"></span>         |
| `silver`               | `#FFC0C0C0` | <span style="background:#C0C0C0" class="color-block"></span>         |
| `sky_blue`             | `#FF87CEEB` | <span style="background:#87CEEB" class="color-block"></span>         |
| `slate_blue`           | `#FF6A5ACD` | <span style="background:#6A5ACD" class="color-block"></span>         |
| `slate_gray`           | `#FF708090` | <span style="background:#708090" class="color-block"></span>         |
| `snow`                 | `#FFFFFAFA` | <span style="background:#FFFAFA" class="color-block"></span>         |
| `spring_green`         | `#FF00FF7F` | <span style="background:#00FF7F" class="color-block"></span>         |
| `steel_blue`           | `#FF4682B4` | <span style="background:#4682B4" class="color-block"></span>         |
| `tan`                  | `#FFD2B48C` | <span style="background:#D2B48C" class="color-block"></span>         |
| `teal`                 | `#FF008080` | <span style="background:#008080" class="color-block"></span>         |
| `thistle`              | `#FFD8BFD8` | <span style="background:#D8BFD8" class="color-block"></span>         |
| `tomato`               | `#FFFF6347` | <span style="background:#FF6347" class="color-block"></span>         |
| `transparent`          | `#00FFFFFF` | <span style="background-color:#00FFFFFF" class="color-block"></span> |
| `turquoise`            | `#FF40E0D0` | <span style="background:#40E0D0" class="color-block"></span>         |
| `violet`               | `#FFEE82EE` | <span style="background:#EE82EE" class="color-block"></span>         |
| `wheat`                | `#FFF5DEB3` | <span style="background:#F5DEB3" class="color-block"></span>         |
| `white`                | `#FFFFFFFF` | <span style="background:#FFFFFF" class="color-block"></span>         |
| `white_smoke`          | `#FFF5F5F5` | <span style="background:#F5F5F5" class="color-block"></span>         |
| `yellow`               | `#FFFFFF00` | <span style="background:#FFFF00" class="color-block"></span>         |
| `yellow_green`         | `#FF9ACD32` | <span style="background:#9ACD32" class="color-block"></span>         |

[!INCLUDE [Commands](../../_includes/plugins/focus-indicator.md)]
