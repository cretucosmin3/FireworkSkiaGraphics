using System;
using System.Linq;

namespace Performance;

public static class Maths
{
    public static float[] Normalize(float[] values, float min, float max)
    {
        if (!values.Any())
        {
            return Array.Empty<float>();
        }

        float currentMin = values.Min();
        float currentMax = values.Max();

        return values.Select(value => NormalizeValue(value, currentMin, currentMax, min, max)).ToArray();
    }

    public static float NormalizeValue(float value, float currentMin, float currentMax, float min, float max)
    {
        return min + (value - currentMin) * (max - min) / (currentMax - currentMin);
    }
}