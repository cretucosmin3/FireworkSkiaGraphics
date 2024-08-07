using System.Text;
using SilkyMetrics.Charts;
using SilkyMetrics.Classes;
using SilkyMetrics.Interfaces;
using SkiaSharp;

namespace SilkyMetrics.Base;

internal class MetricBlock
{
    // Constants
    private const float TextPadding = 8;

    // Computed
    private float TextHeight = 0;

    // Functional
    private readonly ChartBase visualChart;
    private readonly MetricOptions Options;
    private readonly DrawLocation DrawLocation;
    private readonly StringBuilder StrB = new();

    private SKPaint TextPaint = new()
    {
        Color = SKColors.Black,
        TextSize = 16,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName(FontHelper.DefaultFontName,
            new SKFontStyle(500, 2, SKFontStyleSlant.Upright)
        )
    };

    public MetricBlock(MetricOptions options, DrawLocation drawLocation)
    {
        Options = options;
        DrawLocation = drawLocation;

        TextHeight = FontHelper.GetTextHeight();

        if (options.ChartType == ChartType.Bars)
            visualChart = new BarChart();
        else
            visualChart = new LineChart(options.ChartType == ChartType.Hills);

        visualChart.Initialize(options, new DrawLocation()
        {
            Width = drawLocation.Width,
            Height = drawLocation.Height - (TextHeight + TextPadding),
            X = drawLocation.X,
            Y = drawLocation.Y + TextHeight + TextPadding
        });

        TextPaint.Color = options.TextColor;
    }

    public void UpdateValue(float newValue)
    {
        visualChart.AddNewValue(newValue);
    }

    public void Draw(SKCanvas canvas)
    {
        visualChart.Draw(canvas);

        PrepareText();

        float x = DrawLocation.X;
        float y = DrawLocation.Y + TextHeight;

        canvas.DrawText(StrB.ToString(), x, y, TextPaint);
    }

    private void PrepareText()
    {
        StrB.Clear();

        StrB.Append(Options.Label);
        StrB.Append(": ");

        if (Options.Precise)
            StrB.Append(visualChart.LastValue.ToString("0.00"));
        else
            StrB.Append((int)visualChart.LastValue);

        if (!string.IsNullOrEmpty(Options.UnitLabel))
        {
            StrB.Append(' ');
            StrB.Append(Options.UnitLabel);
        }
    }
}