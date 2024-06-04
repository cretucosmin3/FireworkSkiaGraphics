
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SilkyMetrics.Classes;
using SkiaSharp;

namespace SilkyMetrics.Base;

public sealed class GraphMetrics
{
    private GraphMetrics() { }

    public static GraphMetrics Instance { get; } = new GraphMetrics();

    private readonly Stopwatch CPUTimer = new();
    private readonly Stopwatch GPUTimer = new();
    private readonly Dictionary<string, MetricBlock> MetricBlocks = [];

    private MetricsWindow? Window = null;
    private Thread? MainThread;
    private bool WasInitialized = false;

    private bool FPS_Enabled = false;
    private bool CPU_Enabled = false;
    private bool GPU_Enabled = false;
    private bool MEM_Enabled = false;

    private int NextYBlockPosition = 12;
    private readonly int BlocksGap = 14;

    private int WindowWidth = 350;
    private int WindowHeight = 350;

    private void AddChartBlock(MetricOptions options)
    {
        DrawLocation drawLoc = new()
        {
            X = BlocksGap,
            Y = NextYBlockPosition,
            Width = WindowWidth - (BlocksGap * 2),
            Height = options.Height
        };

        var newBlock = new MetricBlock(options, drawLoc);

        MetricBlocks.Add(options.Label, newBlock);

        NextYBlockPosition += options.Height + BlocksGap;
    }

    private void DrawBlocks(SKCanvas canvas)
    {
        foreach (var block in MetricBlocks.Values)
        {
            block.Draw(canvas);
        }
    }

    private void CalculateRequiredWindowHeight(InitializeOptions options)
    {
        int totalHeight = 12;

        if (options.FPS != null)
        {
            totalHeight += options.FPS.Height + BlocksGap;
        }

        if (options.CPU != null)
        {
            totalHeight += options.CPU.Height + BlocksGap;
        }

        if (options.GPU != null)
        {
            totalHeight += options.CPU.Height + BlocksGap;
        }

        if (options.MEM != null)
        {
            totalHeight += options.CPU.Height + BlocksGap;
        }

        foreach (var customMetric in options.CustomMetrics)
        {
            totalHeight += customMetric.Height + BlocksGap;
        }

        WindowHeight = totalHeight;
    }

    public static void Initialize(InitializeOptions options)
    {
        Instance.WindowWidth = options.WindowWidth;

        if (options.FPS != null)
        {
            options.FPS.Label = "FPS";
            Instance.AddChartBlock(options.FPS);
            Instance.FPS_Enabled = true;
        }

        if (options.CPU != null)
        {
            options.CPU.Label = "CPU";
            Instance.AddChartBlock(options.CPU);
            Instance.CPU_Enabled = true;
        }

        if (options.GPU != null)
        {
            options.GPU.Label = "GPU";
            Instance.AddChartBlock(options.GPU);
            Instance.GPU_Enabled = true;
        }

        if (options.MEM != null)
        {
            options.MEM.Label = "MEM";
            Instance.AddChartBlock(options.MEM);
            Instance.MEM_Enabled = true;
        }

        foreach (var customMetric in options.CustomMetrics)
        {
            Instance.AddChartBlock(customMetric);
        }

        Instance.CalculateRequiredWindowHeight(options);

        Instance.WasInitialized = true;
    }

    public static void ShowWindow()
    {
        if (!Instance.WasInitialized)
            throw new Exception("SilkyMetrics was not initialized. Call 'Initialize' method first!");

        if (Instance.MainThread != null && Instance.Window != null && Instance.MainThread.IsAlive && Instance.Window.IsActive)
            return;

        try
        {
            Instance.MainThread = new Thread(() =>
            {
                Instance.Window = new MetricsWindow();
                Instance.Window.Render += Instance.DrawBlocks;
                Instance.Window.Init(Instance.WindowWidth, Instance.WindowHeight);
            });

            Instance.MainThread.Start();

            Instance.MainThread = null;
            Instance.Window = null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.ToString}");
        }
    }

    public static void CloseWindow()
    {
        Instance.Window?.Close();
    }

    public static void Begin()
    {
        Instance.CPUTimer.Restart();
        Instance.GPUTimer.Stop();
    }

    public static void End(float deltaTime)
    {
        if (!Instance.WasInitialized) return;

        Instance.CPUTimer.Stop();

        float cpuTime = Instance.CPUTimer.ElapsedMilliseconds;
        float gpuTime = Instance.GPUTimer.ElapsedMilliseconds;

        if (Instance.FPS_Enabled)
            Instance.MetricBlocks["FPS"].UpdateValue(1000f / (deltaTime * 1000));

        if (Instance.CPU_Enabled)
            Instance.MetricBlocks["CPU"].UpdateValue(cpuTime);

        if (Instance.GPU_Enabled)
            Instance.MetricBlocks["GPU"].UpdateValue(gpuTime);

        Instance.GPUTimer.Restart();
    }

    public static void UpdateMetric(string key, float value)
    {
        if (Instance.WasInitialized)
            Instance.MetricBlocks[key].UpdateValue(value);
    }

    public static void Close()
    {
        Instance.Window?.TriggerClose?.Invoke();
    }
}