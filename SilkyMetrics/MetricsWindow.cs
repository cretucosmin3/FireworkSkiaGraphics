using System;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SkiaSharp;
using System.Threading;

namespace SilkyMetrics.Base;

public class MetricsWindow
{
    private IWindow _window;
    private GL _gl;

    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    private uint _program;

    private Texture MainTexture;

    private int WindowWidth = 350;
    private int WindowHeight = 500;

    private static SKPaint SimplePaint = new()
    {
        Color = SKColors.Purple
    };

    public Action<SKCanvas> Render;
    public Action TriggerClose;

    private bool MustRender = true;
    public void TriggerRender() => MustRender = true;

    private bool RenderLoopActive = false;
    public bool IsActive { get => _window.IsClosing == false && RenderLoopActive; }

    public void Init(int width, int height)
    {
        WindowWidth = width;
        WindowHeight = height;

        WindowOptions options = WindowOptions.Default;

        options.Size = new Vector2D<int>(WindowWidth, WindowHeight);
        options.Title = "Metrics";
        options.WindowState = WindowState.Normal;
        options.WindowBorder = WindowBorder.Fixed;
        options.IsEventDriven = true;
        options.FramesPerSecond = 1;

        options.API = new GraphicsAPI(
           ContextAPI.OpenGL,
           ContextProfile.Core,
           ContextFlags.ForwardCompatible,
           new APIVersion(3, 2)
        );

        _window = Window.Create(options);

        TriggerClose += () =>
        {
            _window.Close();
        };

        _window.Load += OnLoad;
        _window.Render += OnRender;

        _window.Run();
        _window.Dispose();
    }

    public void Close()
    {
        _window.Close();
    }

    void StartWindow()
    {
        RenderLoopActive = true;

        while (!_window.IsClosing)
        {
            _window.DoRender();

            Thread.Sleep(100);
        }

        RenderLoopActive = false;

        MainTexture.Dispose();
        // _window.Dispose();
        // _gl.Dispose();
    }

    private unsafe void OnLoad()
    {
        _window.WindowState = WindowState.Normal;
        _window.Center();

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.White);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        float[] vertices =
        [
            // aPosition        aTexCoords
             1f, -1f, 0.0f,     1.0f, 1.0f,
             1f,  1f, 0.0f,     1.0f, 0.0f,
            -1f,  1f, 0.0f,     0.0f, 0.0f,
            -1f, -1f, 0.0f,     0.0f, 1.0f
        ];

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        uint[] indices =
        [
            0u, 1u, 3u,
            1u, 2u, 3u
        ];

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        Shader normalShader = new(_gl, ShaderData.Fragment, ShaderData.Vertex);

        _program = _gl.CreateProgram();

        normalShader.AttachToProgram(_program);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);

        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        normalShader.DetachFromProgram(_program);
        normalShader.EnableAttributes();

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        MainTexture = new Texture(_gl, WindowWidth, WindowHeight, false);

        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        StartWindow();
    }

    private unsafe void OnRender(double _)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);

        MainTexture.Draw((canvas) =>
        {
            canvas.Clear();
            Render?.Invoke(canvas);
        });

        MainTexture.Render();
    }
}