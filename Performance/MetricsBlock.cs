using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Performance;

public class MetricBlock
{
    public string Label { get; set; }
    public float Value { get; private set; } = 0;
    public bool HasGraphBar { get; set; } = true;
    public int AveragingTicks { get; set; } = 15;

    // Chart and display
    private readonly Queue<float> GraphValues = new();
    private const int MaxQueueSize = 60;

    // Rendering
    private SKPaint Paint = new SKPaint
    {
        Color = SKColors.Black,
        TextSize = 16,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("DejaVu Sans",
            new SKFontStyle(300, 2, SKFontStyleSlant.Upright)
        )
    };

    public SKColor BackBarColor = new SKColor(60, 60, 60);
    public SKColor BarColor = SKColors.Blue;
    public SKColor TextColor = SKColors.Black;

    private float TextHeight = 0;
    private float TextWidth = 0;

    // Position & Size
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }

    public float FullBlockHeight { get => Height + TextHeight + 8; }

    private int currentTick = 0;
    private float cumulativeValue = 0;

    public MetricBlock(string label, float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Label = label;

        GetTextHeight(label);
    }

    private void GetTextHeight(string sample)
    {
        SKRect textRect = default!;
        Paint.MeasureText($"Sample: 0.1 ms", ref textRect);

        TextHeight = textRect.Height;
        TextWidth = textRect.Width;
    }

    private void UpdateChartValue(float newValue)
    {
        if (GraphValues.Count >= MaxQueueSize)
        {
            GraphValues.Dequeue();
        }

        GraphValues.Enqueue(newValue);
    }

    public void UpdateValue(float newValue)
    {
        cumulativeValue += newValue;
        currentTick++;

        if (currentTick >= AveragingTicks)
        {
            Value = cumulativeValue / AveragingTicks;
            cumulativeValue = 0;
            currentTick = 0;

            UpdateChartValue(Value);
        }
    }

    private void DrawGraphBar(SKCanvas canvas)
    {
        if (!HasGraphBar) return;

        Paint.Color = BackBarColor;
        canvas.DrawRect(X, Y, Width, Height, Paint);

        float[] NormalizedGraphValues = Maths.Normalize(GraphValues.ToArray(), 0, Height);

        float GraphBarWidth = Width / MaxQueueSize;
        float GraphBarBottom = Y + Height;

        Paint.Color = BarColor;

        for (int i = 0; i < NormalizedGraphValues.Length; i++)
        {
            float barHeight = NormalizedGraphValues[i];
            float xAdvance = X + (GraphBarWidth * i);

            canvas.DrawRoundRect(xAdvance, GraphBarBottom - barHeight, GraphBarWidth, barHeight, 6, 6, Paint);
        }
    }

    public void Draw(SKCanvas canvas)
    {
        DrawGraphBar(canvas);

        Paint.Color = TextColor;
        canvas.DrawText($"{Label}: {Value:0.0}", X + 2, Y - 8, Paint);
    }
}