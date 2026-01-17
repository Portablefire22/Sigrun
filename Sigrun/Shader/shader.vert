#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Color;

layout(std140, set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(std140, set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout(std140, set = 0, binding = 2) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) out vec2 fsin_Color;

void main()
{
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition; 
    fsin_Color = Color;
}