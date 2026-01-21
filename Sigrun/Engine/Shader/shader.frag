#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) in vec3 fsin_normal;
layout(location = 2) in float fsin_alpha;

layout(set = 2, binding = 0) uniform texture2D Texture;
layout(set = 2, binding = 1) uniform sampler SurfaceSampler;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = texture(sampler2D(Texture, SurfaceSampler), fsin_texCoords);
    fsout_Color.w = fsin_alpha;
}