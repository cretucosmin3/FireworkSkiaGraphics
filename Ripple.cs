using System;
using System.Diagnostics;
using System.Numerics;
using SkiaSharp;

public class Ripple
{
    public bool IsSpecial;
    private float Magnitude;
    private Vector2 Position;
    private Stopwatch clock;

    public float Size = 10;
    public float Distance = 450;
    public float Duration = 250;
    public float BabyChance = 0.80f;
    public float ReverseRatio = 1.2f;

    public SKPaint Paint;

    public bool IsFinished { get => clock.ElapsedMilliseconds > Duration; }
    public float TimeRatio { get => clock.ElapsedMilliseconds / Duration; }

    public Action<float, float> SideEffect;

    public Ripple(Vector2 position, float magnitude)
    {
        Position = position;
        Magnitude = magnitude;
        clock = Stopwatch.StartNew();

        // Factor for Magnitude
        Size = Math.Max(15, Size * magnitude);
        Distance = Math.Min(350, Math.Max(100, Distance * magnitude));
        Duration = Math.Min(1000, Math.Max(300, Duration * magnitude));

        Paint = new SKPaint
        {
            Color = new SKColor(
                (byte)Random.Shared.Next(40, 255),
                (byte)Random.Shared.Next(40, 185),
                (byte)Random.Shared.Next(40, 185),
                (byte)Random.Shared.Next(150, 255)
            ),
        };
    }

    public void Cycle()
    {
        if (!IsSpecial) return;

        if (Random.Shared.NextDouble() < BabyChance)
        {
            float blobRadius = Distance * TimeRatio;
            blobRadius += blobRadius * 0.2f * (float)Random.Shared.NextDouble();

            GetRandomPointOnRadius(blobRadius, Position.X, Position.Y, out float x, out float y);
            SideEffect?.Invoke(x, y);
        }
    }

    public void Draw(SKCanvas canvas)
    {
        float dx = Distance * ReverseRatio;
        float blobRadius = (Distance - (dx * TimeRatio)) * TimeRatio;
        float blobSize = Size - (Size * TimeRatio);
        float blobHalfSize = blobSize / 2f;
        float fuzzIncrease = 0.45f * (blobRadius / Distance);

        for (int i = 0; i < (30 * Magnitude); i++)
        {
            float radiusFuzz = (float)(blobRadius * fuzzIncrease * Random.Shared.NextDouble());
            GetRandomPointOnRadius(blobRadius + radiusFuzz, Position.X, Position.Y, out float x, out float y);

            canvas.DrawRect(x - blobHalfSize, y - blobHalfSize, blobSize, blobSize, Paint);
        }
    }

    private static void GetRandomPointOnRadius(float radius, float xIn, float yIn, out float xOut, out float yOut)
    {
        var angle = Random.Shared.Next(0, 360);

        xOut = xIn + (radius * MathF.Cos(angle));
        yOut = yIn + (radius * MathF.Sin(angle));
    }
}