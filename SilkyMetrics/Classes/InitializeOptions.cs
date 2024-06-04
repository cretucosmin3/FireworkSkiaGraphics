using System.Collections.Generic;
using SkiaSharp;

namespace SilkyMetrics.Classes;

public class InitializeOptions
{
    public int WindowWidth = 350;

    /// <summary>
    /// Background color of the window.
    /// </summary>
    public SKColor BackgroundColor = SKColors.White;

    /// <summary>
    /// If set displays the FPS metric.
    /// </summary>
    public MetricOptions FPS;

    /// <summary>
    /// If set, it displays CPU processing duration in miliseconds.
    /// </summary>
    public MetricOptions CPU;

    /// <summary>
    /// If set, it displays GPU processing duration in miliseconds.
    /// </summary>
    public MetricOptions GPU;

    /// <summary>
    /// If set, displays memory usage in Mb or Gb.
    /// </summary>
    public MetricOptions MEM;

    /// <summary>
    /// Defines a list of custom metrics.
    /// </summary>
    public List<MetricOptions> CustomMetrics = new();
}