public static class ShaderData
{
    public static readonly string Vertex = @"
        #version 330 core
        
        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aTexCoords;
        
        out vec2 frag_texCoords;
        
        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
            frag_texCoords = aTexCoords;
        }
    ";

    public static readonly string Fragment = @"
        #version 330 core
        
        // attributes from our vertex shader
        in vec2 frag_texCoords;
        out vec4 out_color;
        uniform sampler2D uTexture;

        void main()
        {
            out_color = texture(uTexture, frag_texCoords);
        }
    ";
}