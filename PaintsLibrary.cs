using SkiaSharp;

public static class PaintsLibrary
{
    public static SKPaint SimpleBlackText = new SKPaint
    {
        Color = SKColors.Black,
        TextSize = 22,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("DejaVu Sans",
            new SKFontStyle(300, 2, SKFontStyleSlant.Upright)
        )
    };

    public static SKPaint SimpleWhiteText = new SKPaint
    {
        Color = SKColors.White,
        TextSize = 22,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("DejaVu Sans",
            new SKFontStyle(300, 2, SKFontStyleSlant.Upright)
        )
    };

    public static SKPaint MovingCube = new SKPaint 
    {
        Color = SKColors.Black
    };

    public static SKPaint RedMovingCube = new SKPaint 
    {
        Color = SKColors.Red
    };
}