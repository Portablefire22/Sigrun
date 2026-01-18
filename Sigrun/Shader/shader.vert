#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Color;
layout(location = 2) in int texIndex;

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
    //mat4 World;
};


/*layout(std140, set=0, binding=2) buffer ObjectBuffer{
    ObjectData objects;
    //mat4 World;
};*/

layout(location = 0) out vec4 fsin_Color;
layout(location = 1) out int tex_index;

void main()
{
    vec4 worldPosition = objects.transformation * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_Color = vec4(1,1,1,1);
    tex_index = texIndex;
    //fsin_Color = vec4(0.34, 0.2, 1, 1);
}