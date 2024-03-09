
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SkiaSharp;

namespace Performance;

public class PerformanceMetrics
{
    private MetricsWindow Window;
    private Thread MainThread;
    private Stopwatch timer = new Stopwatch();

    private Dictionary<string, MetricBlock> MetricBlocks = new();

    private bool IsShowingFPS = false;
    private bool IsShowingCPU = false;
    private bool IsShowingGPU = false;

    private float NextYBlockPosition = 30f;
    private float BlocksGap = 10f;

    public PerformanceMetrics()
    {
        try
        {
            MainThread = new Thread(() =>
            {
                Window = new MetricsWindow();
                Window.Render += DrawMetricBlocks;
                Window.Init();
            });
            MainThread.Start();
        }
        catch (Exception _)
        {

        }
    }

    public void DrawMetricBlocks(SKCanvas canvas)
    {
        foreach (var block in MetricBlocks.Values)
        {
            block.Draw(canvas);
        }
    }

    #region Public Methods

    public void ShowFPS()
    {
        if (IsShowingFPS) return;

        MetricBlock FPSBlock = new("FPS", 10, NextYBlockPosition, 330, 35)
        {
            BarColor = SKColors.CadetBlue,
            AveragingTicks = 15
        };

        MetricBlocks.Add("FPS", FPSBlock);

        NextYBlockPosition += FPSBlock.FullBlockHeight + BlocksGap;
        IsShowingFPS = true;
    }

    public void ShowCPU()
    {
        if (IsShowingCPU) return;

        MetricBlock CPUBlock = new("CPU", 10, NextYBlockPosition, 330, 35)
        {
            BarColor = SKColors.SeaGreen,
            AveragingTicks = 15
        };

        MetricBlocks.Add("CPU", CPUBlock);

        NextYBlockPosition += CPUBlock.FullBlockHeight + BlocksGap;
        IsShowingCPU = true;
    }

    public void ShowGPU()
    {
        if (IsShowingGPU) return;

        MetricBlock GPUBlock = new("GPU", 10, NextYBlockPosition, 330, 35)
        {
            BarColor = SKColors.IndianRed,
            AveragingTicks = 15
        };

        MetricBlocks.Add("GPU", GPUBlock);

        NextYBlockPosition += GPUBlock.FullBlockHeight + BlocksGap;
        IsShowingGPU = true;
    }

    public void Track()
    {
        timer.Restart();
    }

    public void UpdateDeltaTime(float deltaTime)
    {
        timer.Stop();

        float cpuTime = timer.ElapsedMilliseconds;
        float gpuTime = (deltaTime * 1000) - cpuTime;

        if (IsShowingFPS)
            MetricBlocks["FPS"].UpdateValue(1000f / (deltaTime * 1000));

        if (IsShowingCPU)
            MetricBlocks["CPU"].UpdateValue(cpuTime);

        if (IsShowingGPU)
            MetricBlocks["GPU"].UpdateValue(gpuTime);

    }

    public void AddChartBlock(string key, string label, SKColor color)
    {
        var newBlock = new MetricBlock(label, 10, NextYBlockPosition, 330, 35)
        {
            BarColor = color,
            AveragingTicks = 15
        };

        MetricBlocks.Add(key, newBlock);

        NextYBlockPosition += newBlock.FullBlockHeight + BlocksGap;
    }

    public void UpdateValue(string key, float value)
    {
        MetricBlocks[key].UpdateValue(value);
    }

    public void Close()
    {
        Window.TriggerClose?.Invoke();
    }

    #endregion
}