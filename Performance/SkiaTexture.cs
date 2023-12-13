using System;
using Silk.NET.OpenGL;
using SkiaSharp;

namespace Performance;

public class SkiaGlTexture
{
    private GL _gl;
    private uint _texture;
    private int _width;
    private int _height;
    private SKCanvas _canvas;

    public SkiaGlTexture(GL gl, int width, int height)
    {
        _gl = gl;
        _width = width;
        _height = height;

        CreateTexture();
        // Bind();
    }

    public void CreateCanvasAndBitmap()
    {
        var info = new SKImageInfo(_width, _height, SKColorType.Rgba8888);
    }

    private void CreateTexture()
    {
        _texture = _gl.GenTexture();
        SetParameters();
    }

    public unsafe void Draw(Action<SKCanvas> drawAction)
    {
        Bind();

        _gl.ActiveTexture(TextureUnit.Texture0);

        var info = new SKImageInfo(_width, _height, SKColorType.Rgba8888);
        
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);
        
        drawAction(canvas);

        fixed (byte* ptr = bitmap.Bytes)
        {
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                (uint)_width,
                (uint)_height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }

        Unbind();
    }

    public void SetParameters()
    {
        Bind();

        // This tells the GPU how it should sample the texture.
        _gl.TextureParameter(_texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TextureParameter(_texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // The min and mag filters define how the texture should be sampled as it resized.
        _gl.TextureParameter(_texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TextureParameter(_texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        Unbind();
    }

    public void Bind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
    }

    public void Unbind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public unsafe void Render()
    {
        Bind();
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
}