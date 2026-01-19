#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) flat in int tex_index;

layout(set = 2, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 2, binding = 1) uniform sampler SurfaceSampler;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
}