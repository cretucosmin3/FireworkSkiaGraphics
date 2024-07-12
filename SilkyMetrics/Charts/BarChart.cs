using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SilkyMetrics.Classes;
using SilkyMetrics.Interfaces;
using SilkyMetrics.Base;

namespace SilkyMetrics.Charts;

internal class BarChart : ChartBase
{
    private readonly List<float> GraphValues = new();
    private readonly List<float> TempValues = new();
    private DateTime LastValueTime = DateTime.Now;
    private int loopingValueIndex = 0;

    private MetricOptions Options;
    private DrawLocation DrawLocation;

    private bool SkipFirstValue = true;

    private SKPaint BackPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        PathEffect = SKPathEffect.CreateCorner(3f),
    };

    private SKPaint Paint = new()
    {
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    internal override void Initialize(MetricOptions options, DrawLocation drawLocation)
    {
        Options = options;
        DrawLocation = drawLocation;

        BackPaint.Color = options.BackColor;
        Paint.Color = options.ChartColor;
    }

    internal override void AddNewValue(float newValue)
    {
        if (SkipFirstValue && (DateTime.Now - LastValueTime).TotalSeconds > 2)
        {
            SkipFirstValue = false;
            return;
        }

        if (Options.PlotsEachValue)
        {
            AddValueToGraph(newValue);
            LastValue = newValue;
            return;
        }

        TempValues.Add(newValue);

        if ((DateTime.Now - LastValueTime).TotalSeconds >= Options.ValueTimeWindow)
        {
            float avgValue = TempValues.Count > 0 ? TempValues.Average() : 0;
            LastValue = avgValue;

            TempValues.Clear();

            AddValueToGraph(avgValue);
            LastValueTime = DateTime.Now;
        }
    }

    private void AddValueToGraph(float value)
    {
        if (Options.Continious)
        {
            AddContiniousValue(value);
            return;
        }

        AddLoopingValue(value);
    }

    private void AddContiniousValue(float value)
    {
        if (GraphValues.Count >= Options.MaxValues)
        {
            GraphValues.RemoveAt(0);
        }


        GraphValues.Add(value);
    }

    private void AddLoopingValue(float value)
    {
        if (GraphValues.Count < Options.MaxValues)
        {
            GraphValues.Add(value);
            return;
        }

        GraphValues[loopingValueIndex] = value;

        loopingValueIndex++;
        if (loopingValueIndex == Options.MaxValues) loopingValueIndex = 0;
    }

    internal override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(DrawLocation.X, DrawLocation.Y, DrawLocation.Width, DrawLocation.Height, BackPaint);

        float[] NormalizedGraphValues = Maths.Normalize(GraphValues.ToArray(), 2, DrawLocation.Height - 2);

        float GraphBarWidth = DrawLocation.Width / Options.MaxValues;
        float GraphBarBottom = DrawLocation.Y + DrawLocation.Height;

        for (int i = 0; i < NormalizedGraphValues.Length; i++)
        {
            float barHeight = NormalizedGraphValues[i];
            float xAdvance = DrawLocation.X + (GraphBarWidth * i);

            canvas.DrawRect(xAdvance, GraphBarBottom - barHeight, GraphBarWidth, barHeight, Paint);
        }
    }
}