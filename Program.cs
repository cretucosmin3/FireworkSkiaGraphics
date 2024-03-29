using System;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SkiaSharp;
using System.Diagnostics;
using Silk.NET.Input;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Performance;
using System.Threading;

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

    private static readonly int WindowWidth = (int)(1080 / 1.5f);
    private static readonly int WindowHeight = 1920 / 2;

    private static Vector2 MousePos = new(0, 0);
    private static bool MouseDown = false;

    private static readonly List<Ripple> Ripples = new();
    private static readonly List<Ripple> SmallRipples = new();

    static PerformanceMetrics PerformanceTracker;

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
            PerformanceTracker.Close();
        };

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
        PerformanceTracker = new PerformanceMetrics();

        PerformanceTracker.ShowFPS();
        PerformanceTracker.ShowCPU();
        PerformanceTracker.ShowGPU();

        PerformanceTracker.AddChartBlock("Ripples", "Ripples", SKColors.AliceBlue);

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

        // for (int i = 0; i < TexturePool.Length; i++)
        // {
        //     TexturePool[i] = new Texture(_gl, WindowWidth, WindowHeight, true);

        //     TexturePool[i].Draw((canvas) =>
        //     {
        //         for (int X = 0; X < 200; X++)
        //         {
        //             int randomX = Random.Shared.Next(-20, WindowWidth - 10);
        //             int randomY = Random.Shared.Next(-20, WindowHeight - 10);

        //             canvas.DrawText(
        //                 $"{100 * Random.Shared.NextDouble():0}",
        //                 new SKPoint(randomX + 18 - (i > 9 ? 7 : 0), randomY + 32),
        //                 Random.Shared.NextDouble() > 0.5d ? PaintsLibrary.SimpleBlackText : PaintsLibrary.SimpleWhiteText
        //             );
        //         }
        //     });
        // }

        TimeMeasure.Print($"Each cube on texture render textures");


        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static void OnUpdate(double dt) { }

    

    private static unsafe void OnRender(double frameDelta)
    {
        PerformanceTracker.Track();

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);

        // The quad vertices data.
        float[] vertices =
        {
          // aPosition  --------   aTexCoords
             1f, -1f, 0.0f,      1.0f, 1.0f,
             1f, 1f, 0.0f,      1.0f, 0.0f,
            -1f, 1f, 0.0f,      0.0f, 0.0f,
            -1f, -1f, 0.0f,      0.0f, 1.0f
        };

        // Upload the vertices data to the VBO.
        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);


        // for (int i = 0; i < 1; i++)
        // {
        //     TexturePool[i].Draw((canvas) =>
        //     {
        //         for (int X = 0; X < 200; X++)
        //         {
        //             int randomX = Random.Shared.Next(-20, WindowWidth - 10);
        //             int randomY = Random.Shared.Next(-20, WindowHeight - 10);

        //             canvas.DrawText(
        //                 $"{100 * Random.Shared.NextDouble():0}",
        //                 new SKPoint(randomX + 18 - (i > 9 ? 7 : 0), randomY + 32),
        //                 Random.Shared.NextDouble() > 0.25d ? PaintsLibrary.SimpleBlackText : PaintsLibrary.SimpleWhiteText
        //             );
        //         }
        //     });
        // }

        if (Ripples.Any() || SmallRipples.Any())
        {
            Texture1.Draw((canvas) =>
            {
                canvas.Clear();

                if (Ripples.Any())
                {
                    List<int> ToRemove = new List<int>();

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

                if (SmallRipples.Any())
                {
                    List<int> ToRemove = new List<int>();

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

        // for (int i = 0; i < TexturePool.Length; i++)
        // {
        //     TexturePool[i].Render();
        // }

        Texture1.Render();

        PerformanceTracker.UpdateValue("Ripples", Ripples.Count + SmallRipples.Count);
        PerformanceTracker.UpdateDeltaTime((float)frameDelta);
    }

    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }
}