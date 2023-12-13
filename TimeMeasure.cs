using System;
using System.Diagnostics;

public static class TimeMeasure
{
    private static readonly Stopwatch timer = new Stopwatch();
    private static float Marks = 0;
    private static long TotalMarks = 0;

    public static void Track()
    {
        timer.Restart();
    }

    public static void Mark()
    {
        timer.Stop();

        TotalMarks += timer.ElapsedMilliseconds;
        Marks++;
    }

    public static void Print(string area)
    {
        timer.Stop();

        float result = Marks > 0 ? TotalMarks / Marks : timer.ElapsedMilliseconds;

        Console.WriteLine($"{area}: took {result:0.00}ms");

        TotalMarks = 0;
        Marks = 0;
    }
}