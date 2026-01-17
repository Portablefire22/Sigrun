#version 450

layout(location = 0) in vec2 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = vec4(0.5,0.32, 1, 1);
}