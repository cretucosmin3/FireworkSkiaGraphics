using System;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SkiaSharp;
using Silk.NET.Input;
using System.Threading;

namespace Performance;

public class MetricsWindow
{
    private IWindow _window;
    private GL _gl;

    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    private uint _program;

    private Texture MainTexture;

    private readonly int WindowWidth = 350;
    private readonly int WindowHeight = 500;


    public Action<SKCanvas> Render;
    public Action TriggerClose;

    public void Init()
    {
        WindowOptions options = WindowOptions.Default;

        options.Size = new Vector2D<int>(WindowWidth, WindowHeight);
        options.Title = "Metrics";
        options.WindowState = WindowState.Normal;
        options.WindowBorder = WindowBorder.Fixed;
        options.IsEventDriven = true;

        options.API = new GraphicsAPI(
           ContextAPI.OpenGL,
           ContextProfile.Core,
           ContextFlags.ForwardCompatible,
           new APIVersion(3, 2)
        );

        Window.PrioritizeSdl();

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

    void StartWindow()
    {
        while (!_window.IsClosing)
        {
            // Not needed but for later use
            _window.DoEvents();
            _window.ContinueEvents();
            _window.DoRender();

            Thread.Sleep(30);
        }

        _window.Dispose();
    }

    private void SetInput()
    {
        // Might be useful to add different views
        // controlled by F1, F2 ...
        // but right now, too early.

        // IInputContext _Input = _window.CreateInput();

        // foreach (var mouse in _Input.Mice)
        // {
        //     mouse.MouseDown += (mouse, button) =>
        //     {

        //     };

        //     mouse.MouseUp += (mouse, button) =>
        //     {

        //     };

        //     mouse.MouseMove += (mouse, position) =>
        //     {

        //     };
        // }
    }

    private unsafe void OnLoad()
    {
        _window.WindowState = WindowState.Normal;
        _window.Center();

        SetInput();

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.White);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        float[] vertices =
        {
              // aPosition          aTexCoords
                 1f, -1f, 0.0f,     1.0f, 1.0f,
                 1f,  1f, 0.0f,     1.0f, 0.0f,
                -1f,  1f, 0.0f,     0.0f, 0.0f,
                -1f, -1f, 0.0f,     0.0f, 1.0f
            };

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        uint[] indices =
        {
                0u, 1u, 3u,
                1u, 2u, 3u
            };

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        Shader normalShader = new Shader(_gl, ShaderData.Fragment, ShaderData.Vertex);

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

    public static SKPaint SimplePaint = new SKPaint
    {
        Color = SKColors.Purple
    };

    private unsafe void OnRender(double frameDelta)
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