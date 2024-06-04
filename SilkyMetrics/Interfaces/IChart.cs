using SilkyMetrics.Classes;
using SkiaSharp;

namespace SilkyMetrics.Interfaces;

public abstract class ChartBase
{
    internal float LatestValue = 0f;
    internal abstract void Initialize(MetricOptions options, DrawLocation drawLocation);
    abstract internal void Draw(SKCanvas canvas);
    abstract internal void AddNewValue(float newValue);
}