using SilkyMetrics.Classes;
using SkiaSharp;

namespace SilkyMetrics.Interfaces;

public abstract class ChartBase
{
    public float LastValue { get; set; } = 0f;
    internal abstract void Initialize(MetricOptions options, DrawLocation drawLocation);
    internal abstract void Draw(SKCanvas canvas);
    internal abstract void AddNewValue(float newValue);
}