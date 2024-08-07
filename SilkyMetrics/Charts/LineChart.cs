using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SilkyMetrics.Classes;
using SilkyMetrics.Interfaces;
using SilkyMetrics.Base;

namespace SilkyMetrics.Charts;

internal class LineChart : ChartBase
{
    private readonly List<float> GraphValues = new();
    private readonly List<float> TempValues = new();
    private DateTime LastValueTime = DateTime.Now;
    private int loopingValueIndex = 0;
    private bool ClosedPath = false;

    private MetricOptions Options;
    private DrawLocation DrawLocation;

    private bool SkipFirstValue = true;

    private SKPaint Paint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        PathEffect = SKPathEffect.CreateCorner(3f),
        StrokeCap = SKStrokeCap.Square,
        StrokeWidth = 2.5f,
    };

    internal LineChart(bool closedPath)
    {
        ClosedPath = closedPath;
    }

    internal override void Initialize(MetricOptions options, DrawLocation drawLocation)
    {
        Options = options;
        DrawLocation = drawLocation;
    }

    internal override void AddNewValue(float newValue)
    {
        if (SkipFirstValue && (DateTime.Now - LastValueTime).TotalSeconds < 1)
        {
            return;
        }

        SkipFirstValue = false;

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
        Paint.Color = Options.BackColor;
        Paint.Style = SKPaintStyle.Fill;

        canvas.DrawRect(DrawLocation.X, DrawLocation.Y, DrawLocation.Width, DrawLocation.Height, Paint);

        if (GraphValues.Count == 0) return;

        float[] NormalizedGraphValues = Maths.Normalize(GraphValues.ToArray(), 2.5f, DrawLocation.Height - 2);

        float GraphLineWidth = DrawLocation.Width / (Options.MaxValues - 1);
        float GraphBottom = DrawLocation.Y + DrawLocation.Height;

        Paint.Color = Options.ChartColor;

        using var path = new SKPath();

        if (ClosedPath)
            path.MoveTo(DrawLocation.X, GraphBottom - 1);

        int i;

        for (i = 0; i < NormalizedGraphValues.Length; i++)
        {
            float x = DrawLocation.X + (GraphLineWidth * i);
            float y = GraphBottom - NormalizedGraphValues[i];

            if (i == 0 && !ClosedPath)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }

        i--;

        if (ClosedPath && NormalizedGraphValues.Length > 1)
        {
            path.LineTo((DrawLocation.X + (GraphLineWidth * i)), GraphBottom - 1);
            // path.LineTo(DrawLocation.X + 1, GraphBottom - 1);
            path.Close();
        }

        Paint.Style = ClosedPath ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
        canvas.DrawPath(path, Paint);
    }
}