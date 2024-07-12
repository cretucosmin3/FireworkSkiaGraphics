using SkiaSharp;

namespace SilkyMetrics.Classes;

public class MetricOptions
{
    /// <summary>
    /// For the built in metrics (FPS, CPU, GPU, MEM) this value is overridden.
    /// </summary>
    public string Label = "";

    /// <summary>
    /// For the built in metrics (FPS, CPU, GPU, MEM) this value is overridden.
    /// </summary>
    public string UnitLabel = null;

    /// <summary>
    /// If <b>true</b> it'll display value with 2 decimals (0.00)
    /// </summary>
    public bool Precise = true;

    /// <summary>
    /// if <b>false</b> the plotting will reset to 0 once the maxValues is hit.
    /// <br/>
    /// <hr/>
    /// <b>Default:</b> <i>true</i>
    /// </summary>
    public bool Continious = true;

    /// <summary>
    /// For the built in metrics (FPS, CPU, GPU, MEM) this value is overridden.
    /// </summary>
    public bool PlotsEachValue = false;
    public float ValueTimeWindow = 0.35f;
    public int Height = 55;
    public int MaxValues = 40;

    // Colors
    public SKColor TextColor = SKColors.Black;
    public SKColor BackColor = new(200, 200, 210);
    public SKColor ChartColor = SKColors.Black;
    public ChartType ChartType = ChartType.Bars;
}