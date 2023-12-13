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

    private float Size = 30;
    private float Distance = 300;
    private float Duration = 800;

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

        if (Random.Shared.NextDouble() < 0.15)
        {
            float blobRadius = Distance * TimeRatio;
            blobRadius += blobRadius * 0.3f * (float)Random.Shared.NextDouble();

            GetRandomPointOnRadius(blobRadius, Position.X, Position.Y, out float x, out float y);
            SideEffect?.Invoke(x, y);
        }
    }

    public void Draw(SKCanvas canvas)
    {
        float blobRadius = Distance * TimeRatio;
        float blobSize = Size - (Size * TimeRatio);
        float blobHalfSize = blobSize / 2f;

        for (int i = 0; i < (40 * Magnitude); i++)
        {
            float radiusFuzz = (float)(blobRadius * 0.15f * Random.Shared.NextDouble());
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