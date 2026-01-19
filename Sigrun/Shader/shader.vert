#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Color;
layout(location = 2) in float in_alpha;

layout(location = 0) out vec2 fsin_texCoords;
layout(location = 1) out float fsin_alpha;


layout(std140, set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(std140, set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

struct ObjectData {
    mat4 transformation;
};

layout(std140, set = 0, binding = 2) uniform WorldBuffer
{
    ObjectData objects;
};
void main()
{
    vec4 worldPosition = objects.transformation * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = Color;
    fsin_alpha = in_alpha;
}