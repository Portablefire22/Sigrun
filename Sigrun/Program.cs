using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Vulkan;

namespace Sigrun;

class Program
{

    private static GraphicsDevice _graphicsDevice;
    private static CommandList _commandList;
    private static DeviceBuffer _vertexBuffer;
    private static DeviceBuffer _indexBuffer;
    private static Shader[] _shaders;
    private static Pipeline _pipeline;
    
    private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

    private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";
    
    static void Main(string[] args)
    {
        var windowCreateInfo = new WindowCreateInfo()
        {
            X = 100,
            Y = 100,
            WindowWidth = 960,
            WindowHeight = 540,
            WindowTitle = "Sigrun"
        };
        var window = VeldridStartup.CreateWindow(ref windowCreateInfo);
        var options = new GraphicsDeviceOptions()
        {
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
        };
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);

        CreateResources();
        
        while (window.Exists)
        {
            window.PumpEvents();
            Draw();
        }
    }
    
    private static void CreateResources()
    {
        var factory = _graphicsDevice.ResourceFactory;

        VertexPositionColour[] quadVertices =
        {
            new VertexPositionColour(new Vector2(-0.75f, 0.75f), RgbaFloat.Red),
            new VertexPositionColour(new Vector2(0.75f, 0.75f), RgbaFloat.Blue),
            new VertexPositionColour(new Vector2(-0.75f, -0.75f), RgbaFloat.Green),
            new VertexPositionColour(new Vector2(0.75f, -0.75f), RgbaFloat.Yellow),
        };
        ushort[] quadIndices = [0, 1, 2, 3];
        _vertexBuffer =
            factory.CreateBuffer(new BufferDescription(4 * VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));
        
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

        var vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float2), new VertexElementDescription("Colour",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float4));
        
        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main");

        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        var pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        pipelineDescription.ResourceLayouts = [];
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, _shaders);
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _commandList = factory.CreateCommandList();
        
    }

    private static void Draw()
    {
        _commandList.Begin();
        
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        _commandList.SetPipeline(_pipeline);

        _commandList.DrawIndexed(4, 1, 0, 0, 0);
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    private static void Dispose()
    {
        _pipeline.Dispose();
        _commandList.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _graphicsDevice.Dispose();
    }
}

struct VertexPositionColour
{
    public Vector2 Position;
    public RgbaFloat Colour;
    public const uint SizeInBytes = 24;

    public VertexPositionColour(Vector2 position, RgbaFloat colour)
    {
        Position = position;
        Colour = colour;
    }
}