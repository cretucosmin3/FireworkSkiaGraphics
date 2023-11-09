using System;
using Silk.NET.OpenGL;

public class Shader
{
    private GL _gl;
    private uint vertexShader;
    private uint fragmentShader;

    public Shader(GL gl, string fragment, string vertex)
    {
        _gl = gl;

        CreateVertexShader(vertex);
        CreateFragmentShader(fragment);
    }

    private void CreateVertexShader(string vertex)
    {
        vertexShader = _gl.CreateShader(ShaderType.VertexShader);

        _gl.ShaderSource(vertexShader, vertex);
        _gl.CompileShader(vertexShader);
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);

        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
    }

    private void CreateFragmentShader(string fragment)
    {
        fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);

        _gl.ShaderSource(fragmentShader, fragment);
        _gl.CompileShader(fragmentShader);
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);

        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
    }

    public unsafe void EnableAttributes()
    {
        // Our stride constant. The stride must be in bytes, so we take the first attribute (a vec3), multiply it
        // by the size in bytes of a float, and then take our second attribute (a vec2), and do the same.
        const uint stride = (3 * sizeof(float)) + (2 * sizeof(float));

        // Enable the "aPosition" attribute in our vertex array, providing its size and stride too.
        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

        // Now we need to enable our texture coordinates!
        const uint textureLoc = 1;
        _gl.EnableVertexAttribArray(textureLoc);
        _gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
    }

    public void AttachToProgram(uint program)
    {
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
    }

    public void DetachFromProgram(uint program)
    {
        _gl.DetachShader(program, vertexShader);
        _gl.DetachShader(program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }
}