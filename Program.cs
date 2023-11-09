using System;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SkiaSharp;
using System.Diagnostics;
using Silk.NET.Input;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tutorial
{
    public class Program
    {
        private static IWindow _window;
        private static GL _gl;

        private static uint _vao;
        private static uint _vbo;
        private static uint _ebo;

        private static uint _program;
        private static uint _texture;

        private static Texture Texture1;

        private static Texture[] TexturePool = new Texture[0];

        private static readonly int WindowWidth = 1920;
        private static readonly int WindowHeight = 1080;

        private static Vector2 MousePos = new Vector2(0, 0);
        private static bool MouseDown = false;

        private static List<Ripple> Ripples = new List<Ripple>();
        private static List<Ripple> SmallRipples = new List<Ripple>();

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(WindowWidth, WindowHeight);
            options.Title = "Textures";
            options.FramesPerSecond = 60;

            _window = Window.Create(options);

            Window.PrioritizeGlfw();

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.FramebufferResize += OnResize;

            _window.Run();

            _window.Dispose();
        }

        private static void SpawnSmallRipple(float x, float y)
        {
            float magnitude = 0.1f + (float)(0.5d * Random.Shared.NextDouble());
            SmallRipples.Add(new Ripple(new Vector2(x, y), magnitude));
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
                        var newRipple = new Ripple(new Vector2(MousePos.X, MousePos.Y), magnitude);

                        newRipple.IsSpecial = button == MouseButton.Right;

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
                };
            }
        }

        private static unsafe void OnLoad()
        {
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

            for (int i = 0; i < TexturePool.Length; i++)
            {
                TexturePool[i] = new Texture(_gl, WindowWidth, WindowHeight);

                TexturePool[i].Draw((canvas) =>
                {
                    var layerPaint = new SKPaint
                    {
                        Color = new SKColor(
                            (byte)Random.Shared.Next(0, 255),
                            (byte)Random.Shared.Next(0, 255),
                            (byte)Random.Shared.Next(0, 255),
                            150
                        )
                    };

                    for (int X = 0; X < 50; X++)
                    {
                        int randomX = Random.Shared.Next(0, WindowWidth - 50);
                        int randomY = Random.Shared.Next(31, WindowHeight - 50);

                        canvas.DrawRect(randomX, randomY, 50, 50, layerPaint);

                        canvas.DrawText(
                            i.ToString(),
                            new SKPoint(randomX + 18 - (i > 9 ? 7 : 0), randomY + 32),
                            PaintsLibrary.SimpleWhiteText
                        );
                    }
                });
            }

            int location = _gl.GetUniformLocation(_program, "uTexture");
            _gl.Uniform1(location, 0);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private static void OnUpdate(double dt) { }

        static Stopwatch timer = new Stopwatch();
        static float angle = 0f; // Current angle of the object in radians
        static float speed = 4f; // Speed at which the object moves along the circle (radians per second)

        // Parameters for the circular motion
        static float centerX = 500f; // X coordinate of the circle's center
        static float centerY = 400f; // Y coordinate of the circle's center
        static float radius = 200f; // Radius of the circle

        static double fixedAverageFrame = 0;
        static double averageFrame = 0;

        static double fixedAverageDelta = 0;
        static double averageDelta = 0;
        static int frameCounter = 0;

        private static unsafe void OnRender(double frameDelta)
        {
            frameDelta *= 1000;
            if (frameCounter == 45) // 1.5 seconds (30fps)
            {
                fixedAverageFrame = averageFrame / 45;
                fixedAverageDelta = averageDelta / 45;

                averageFrame = 0;
                averageDelta = 0;

                frameCounter = 0;
            }

            averageFrame += timer.ElapsedMilliseconds;
            averageDelta += frameDelta;

            timer.Restart();
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _gl.BindVertexArray(_vao);
            _gl.UseProgram(_program);

            for (int i = 0; i < TexturePool.Length; i++)
            {
                TexturePool[i].Render();
            }

            Texture1.Render();

            Texture1.Draw((canvas) =>
            {
                canvas.Clear();

                // angle += speed * (float)frameDelta;

                // float x = centerX + (radius * MathF.Cos(angle));
                // float y = centerY + (radius * MathF.Sin(angle));

                // float rectX = x - 60;
                // float rectY = y - 60;

                // canvas.DrawRect(0, 0, 800, 60, PaintsLibrary.WhiteMovingCube);
                // canvas.DrawRect(rectX, rectY, 60, 60, PaintsLibrary.MovingCube);

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

                // if (MouseDown)
                // {
                //     for (int i = 0; i < 30; i++)
                //     {
                //         GetRandomPointOnRadius(200, MousePos.X, MousePos.Y, out float x, out float y);
                //         canvas.DrawRect(x - 5, y - 5, 10, 10, MouseDown ? PaintsLibrary.RedMovingCube : PaintsLibrary.MovingCube);
                //     }
                // }


                // for (int X = 0; X < 50; X++)
                // {
                //     int randomX = Random.Shared.Next(0, WindowWidth - 50);
                //     int randomY = Random.Shared.Next(28, WindowHeight - 50);

                //     canvas.DrawRect(randomX, randomY, 50, 50, PaintsLibrary.MovingCube);
                // }

                canvas.DrawText($"Render Function: {fixedAverageFrame:0.00}ms | Delta: {fixedAverageDelta:0.00}ms", 10, 24, PaintsLibrary.SimpleBlackText);
            });

            timer.Stop();
            frameCounter++;
        }

        private static void OnResize(Vector2D<int> size)
        {
            Console.WriteLine(size.ToString());
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }
    }

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

            if (Random.Shared.NextDouble() < 0.1)
            {
                float blobRadius = (Distance * TimeRatio);
                blobRadius += (blobRadius * 0.3f) * (float)Random.Shared.NextDouble();

                GetRandomPointOnRadius(blobRadius, Position.X, Position.Y, out float x, out float y);
                SideEffect?.Invoke(x, y);
            }
        }

        public void Draw(SKCanvas canvas)
        {
            float blobRadius = (Distance * TimeRatio);
            float blobSize = Size - (Size * TimeRatio);
            float blobHalfSize = blobSize / 2f;

            for (int i = 0; i < (40 * Magnitude); i++)
            {
                float radiusFuzz = (float)((blobRadius * 0.05f) * Random.Shared.NextDouble());
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
}