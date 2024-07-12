using System;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using System.Numerics;
using System.Collections.Generic;
using SilkyMetrics;
using SilkyMetrics.Classes;
using SkiaSharp;

namespace RenderTest;

public class Program
{
    private static IWindow _window;
    private static GL _gl;

    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;

    private static uint _program;

    private static Texture Texture1;

    private static readonly Texture[] TexturePool = new Texture[10];

    private static readonly int WindowWidth = 950;
    private static readonly int WindowHeight = 950;

    private static Vector2 MousePos = new(0, 0);
    private static bool MouseDown = false;

    private static readonly List<Ripple> Ripples = new();
    private static readonly List<Ripple> SmallRipples = new();

    public static void Main()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(WindowWidth, WindowHeight);
        options.Title = "Fireworks";
        options.VSync = true;
        options.WindowState = WindowState.Normal;
        options.TransparentFramebuffer = false;
        options.WindowBorder = WindowBorder.Resizable;
        options.PreferredDepthBufferBits = null;
        options.IsEventDriven = false;

        options.API = new GraphicsAPI(
           ContextAPI.OpenGL,
           ContextProfile.Core,
           ContextFlags.ForwardCompatible,
           new APIVersion(3, 2)
        );

        Window.PrioritizeGlfw();

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += () =>
        {
            GraphMetrics.Close();
        };

        GraphMetrics.Initialize(new()
        {
            BackgroundColor = new(80, 80, 85),
            FPS = new()
            {
                Precise = false,
                ValueTimeWindow = 0.1f,
                ChartType = ChartType.Line,
                MaxValues = 40,
                TextColor = SKColors.White,
                ChartColor = SKColors.BlueViolet,
                BackColor = new(25, 25, 25)
            },
            CPU = new()
            {
                ValueTimeWindow = 0.1f,
                ChartType = ChartType.Hills,
                TextColor = SKColors.White,
                ChartColor = SKColors.Aquamarine,
                BackColor = new(25, 25, 25)
            },
            GPU = new()
            {
                ValueTimeWindow = 0.3f,
                MaxValues = 30,
                TextColor = SKColors.White,
                ChartColor = SKColors.IndianRed,
                BackColor = new(25, 25, 25)
            },
            MEM = new()
            {
                ValueTimeWindow = 0.3f,
                MaxValues = 60,
                Height = 80,
                ChartType = ChartType.Bars,
                TextColor = SKColors.White,
                ChartColor = SKColors.Cornsilk,
                BackColor = new(25, 25, 25)
            },
            CustomMetrics = [
                new() {
                    Label = "Ripples",
                    Precise = false,
                    Height = 75,
                    ValueTimeWindow = 0.05f,
                    MaxValues = 50,
                    ChartType = ChartType.Line,
                    TextColor = SKColors.White,
                    ChartColor = SKColors.AliceBlue,
                    BackColor = new(25, 25, 25)
                }
            ]
        });

        // GraphMetrics.Initialize(new()
        // {
        //     FPS = new(),
        //     CPU = new(),
        //     GPU = new(),
        //     MEM = new(),
        //     CustomMetrics = [
        //         new() {
        //             Label = "Ripples",
        //         }
        //     ]
        // });

        _window.Run();

        _window.Dispose();
    }

    private static void SpawnSmallRipple(float x, float y)
    {
        float magnitude = 0.1f + (float)(0.5d * Random.Shared.NextDouble());
        SmallRipples.Add(new Ripple(new Vector2(x, y), magnitude)
        {
            BabyChance = 0.8f,
        });
    }

    private static void SpawnSmallRippleX1(float x, float y)
    {
        float magnitude = 0.1f + (float)(0.5d * Random.Shared.NextDouble());
        SmallRipples.Add(new Ripple(new Vector2(x, y), magnitude)
        {
            BabyChance = 0.02f,
            Duration = 300,
            Distance = 250,
            Size = 12
        });
    }

    private static void SetInput()
    {
        IInputContext _Input = _window.CreateInput();

        foreach (var mouse in _Input.Mice)
        {
            mouse.MouseDown += (mouse, button) =>
            {
                if (!MouseDown)
                {
                    float magnitude = 0.4f + (float)(1.1d * Random.Shared.NextDouble());
                    var newRipple = new Ripple(new Vector2(MousePos.X, MousePos.Y), magnitude)
                    {
                        IsSpecial = button == MouseButton.Right
                    };

                    newRipple.SideEffect += SpawnSmallRipple;

                    Ripples.Add(newRipple);
                }

                MouseDown = true;
            };

            mouse.MouseUp += (mouse, button) =>
            {
                MouseDown = false;
            };

            mouse.MouseMove += (mouse, position) =>
            {
                MousePos = position;

                if (MouseDown)
                {
                    float magnitude = 0.4f + (float)(1.1d * Random.Shared.NextDouble());
                    var newRipple = new Ripple(new Vector2(MousePos.X, MousePos.Y), magnitude)
                    {
                        IsSpecial = true,
                        ReverseRatio = 0.8f,
                        Duration = 150 + (200 * (float)Random.Shared.NextDouble()),
                        Distance = 100 + (100 * (float)Random.Shared.NextDouble()),
                        Size = 15,
                        BabyChance = 0.05f
                    };

                    newRipple.SideEffect += SpawnSmallRippleX1;

                    Ripples.Add(newRipple);
                }
            };
        }
    }

    private static unsafe void OnLoad()
    {
        _window.WindowState = WindowState.Normal;
        _window.Center();

        SetInput();

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.White);

        // Create the VAO.
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // The quad vertices data.
        float[] vertices =
        {
              // aPosition  --------   aTexCoords
                 1f, -1f, 0.0f,      1.0f, 1.0f,
                 1f, 1f, 0.0f,      1.0f, 0.0f,
                -1f, 1f, 0.0f,      0.0f, 0.0f,
                -1f, -1f, 0.0f,      0.0f, 1.0f
            };

        // Create the VBO.
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Upload the vertices data to the VBO.
        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        // The quad indices data.
        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        // Create the EBO.
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // Upload the indices data to the EBO.
        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        Shader normalShader = new Shader(_gl, ShaderData.Fragment, ShaderData.Vertex);

        // Create our shader program, and attach the vertex & fragment shaders.
        _program = _gl.CreateProgram();

        normalShader.AttachToProgram(_program);

        // Attempt to "link" the program together.
        _gl.LinkProgram(_program);

        // Similar to shader compilation, check to make sure that the shader program has linked properly.
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        normalShader.DetachFromProgram(_program);
        normalShader.EnableAttributes();

        // Unbind everything as we don't need it.
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        Texture1 = new Texture(_gl, WindowWidth, WindowHeight, true);

        Texture1.Draw(e => e.Clear());

        TimeMeasure.Print($"Each cube on texture render textures");

        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GraphMetrics.ShowWindow();
    }

    private static void OnUpdate(double dt) { }

    private static unsafe void OnRender(double frameDelta)
    {
        GraphMetrics.Begin();

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);

        // The quad vertices data.
        float[] vertices =
        [
          // aPosition  --------   aTexCoords
             1f, -1f, 0.0f,      1.0f, 1.0f,
             1f, 1f, 0.0f,      1.0f, 0.0f,
            -1f, 1f, 0.0f,      0.0f, 0.0f,
            -1f, -1f, 0.0f,      0.0f, 1.0f
        ];

        // Upload the vertices data to the VBO.
        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        if (Ripples.Count != 0 || SmallRipples.Count != 0)
        {
            Texture1.Draw((canvas) =>
            {
                canvas.Clear();

                if (Ripples.Count != 0)
                {
                    List<int> ToRemove = new();

                    for (int i = 0; i < Ripples.Count; i++)
                    {
                        var ripple = Ripples[i];

                        if (ripple.IsFinished)
                        {
                            ToRemove.Add(i);
                            continue;
                        }

                        ripple.Cycle();
                        ripple.Draw(canvas);
                    }

                    for (int i = ToRemove.Count - 1; i >= 0; i--)
                    {
                        Ripples.RemoveAt(ToRemove[i]);
                    }
                }

                if (SmallRipples.Count != 0)
                {
                    List<int> ToRemove = new();

                    for (int i = 0; i < SmallRipples.Count; i++)
                    {
                        var ripple = SmallRipples[i];

                        if (ripple.IsFinished)
                        {
                            ToRemove.Add(i);
                            continue;
                        }

                        ripple.Draw(canvas);
                    }

                    // Remove items in reverse order
                    for (int i = ToRemove.Count - 1; i >= 0; i--)
                    {
                        SmallRipples.RemoveAt(ToRemove[i]);
                    }
                }
            });
        }

        Texture1.Render();

        GraphMetrics.UpdateMetric("Ripples", Ripples.Count + SmallRipples.Count);
        GraphMetrics.End((float)frameDelta);
    }

    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }
}