using System;
using SkiaSharp;

namespace SilkyMetrics.Classes;

internal static class FontHelper
{
    public static string DefaultFontName
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return "Segoe UI";
            else if (OperatingSystem.IsLinux())
                return "DejaVu Sans";

            // Fallback
            return "Arial";
        }
    }

    public static float GetTextHeight()
    {
        var typeFace = SKTypeface.FromFamilyName(
            FontHelper.DefaultFontName,
            new SKFontStyle(500, 2, SKFontStyleSlant.Upright)
        );

        SKPaint Paint = new()
        {
            TextSize = 16,
            Typeface = typeFace
        };

        SKRect textRect = default!;
        Paint.MeasureText($"X|Z", ref textRect);

        return textRect.Height;
    }
}