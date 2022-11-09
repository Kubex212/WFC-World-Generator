using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public Color GetDarkerColor(Color color, float percentage)
    {
        return new Color(color.r * (1 - percentage), color.g * (1 - percentage), color.b * (1 - percentage), 1);
    }
}

public static class Extensions
{
    public static Color Darker(this Color color, float percentage)
    {
        return new Color(color.r * (1-percentage), color.g * (1 - percentage), color.b * (1 - percentage), 1);
    }
}

