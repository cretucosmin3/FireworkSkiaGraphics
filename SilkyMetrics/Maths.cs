using System.Linq;

namespace SilkyMetrics.Base;

internal static class Maths
{
    public static float[] Normalize(float[] values, float min, float max)
    {
        if (values.Length == 0) return [];

        float currentMin = values.Min();
        float currentMax = values.Max();

        if (currentMax == 0) currentMax = 1;

        return values.Select(value => NormalizeValue(value, currentMin, currentMax, min, max)).ToArray();
    }

    public static float NormalizeValue(float value, float currentMin, float currentMax, float min, float max)
    {
        return min + (value - currentMin) * (max - min) / (currentMax - currentMin);
    }
}