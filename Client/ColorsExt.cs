using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class ColorsExt
{
    public static List<Color> ColorList
        = GetColors();

    public static List<Color> Rainbow
        = GetRainbow();

    private static List<Color> GetRainbow()
    {
        var list = new List<Color>();
        list.Add(Colors.Red);
        list.Add(Colors.Orange);
        list.Add(Colors.Yellow);
        list.Add(Colors.Green);
        list.Add(Colors.Blue);
        list.Add(Colors.Purple);
        return list;
    }

    public static Color GetRainbowColor(int index)
    {
        return Rainbow[index % Rainbow.Count];
    }

    public static Color GetColor(int index)
    {
        return ColorList[index % ColorList.Count];
    }
    private static List<Color> GetColors()
    {
        var colors = new List<Color>();
        var assembly = Assembly.GetExecutingAssembly();
        var godotColorsType = typeof(Colors);
        var props = godotColorsType.GetProperties();
        var colorType = typeof(Color);

        foreach (var prop in props)
        {
            if (prop.PropertyType == colorType)
            {
                colors.Add((Color)prop.GetValue(null));
            }
        }

        return colors;
    }
}
