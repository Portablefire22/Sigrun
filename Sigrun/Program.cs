using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Sigrun.Model.Loader;
using Sigrun.Player;
using Sigrun.Time;
using Veldrid;
using Veldrid.Sdl2;
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
    private static DeviceBuffer _projectionBuffer;
    private static DeviceBuffer _viewBuffer;
    private static DeviceBuffer _worldBuffer;
    private static ResourceSet _projViewSet;
    private static ResourceSet _worldSet;

    private static uint _indexCount;
    
    private static Shader[] _shaders;
    private static Pipeline _pipeline;

    private static float _ticks;
    
    private static Sdl2Window _window;
    
    private static Camera _mainCamera;
    
    
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
        _window = VeldridStartup.CreateWindow(ref windowCreateInfo);
        var options = new GraphicsDeviceOptions()
        {
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            SwapchainDepthFormat = PixelFormat.R16_UNorm
        };
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options);

        CreateResources();

        _mainCamera = new Camera(new Vector3(0,-10, 0), new Vector3(0));
        
        _window.MouseMove += _mainCamera.OnMouseMove;
        _window.KeyDown += _mainCamera.OnKeyDown;

        
        _window.FocusGained += OnFocusGained;
        _window.FocusLost += OnFocusLost;

      
        
        
        while (_window.Exists)
        {
            TimeHandler.UpdateDeltaTime();
            _window.PumpEvents();
            Draw();
        }
    }

    private static void OnFocusLost()
    {
        Sdl2Native.SDL_CaptureMouse(false);
    }

    private static void OnFocusGained()
    {
        Sdl2Native.SDL_CaptureMouse(true);
    }


    private static void CreateResources()
    {
        var factory = _graphicsDevice.ResourceFactory;

        var loader = new ObjLoader();
        // var m = loader.LoadFromFile("C:\\Users\\blake\\Downloads\\cube.txt");
        var m = loader.LoadFromFile("D:\\scp\\Converted\\source\\source\\4tunnels.obj");

        var v = new List<VertexPositionTexture>();
        foreach (var vert in m.Vertices)
        {
            v.Add(new VertexPositionTexture(vert.Position/100, vert.Uv));
        }

        // v = new List<VertexPositionTexture>()
        // {
        //     new VertexPositionTexture(new Vector3(-20f, -20f, 1), new Vector2(0, 0)),
        //     new VertexPositionTexture(new Vector3(20f, -20f, 1), new Vector2(0, 0)),
        //     new VertexPositionTexture(new Vector3(0, 20f, 1), new Vector2(0, 0)),
        // };
        //
        // m.Indices = new ushort[] { 0, 1, 2 };
        
        var vertexArray = v.ToArray();
        _indexCount = (uint) m.Indices.Length;
        
        _vertexBuffer =
            factory.CreateBuffer(new BufferDescription((uint)v.Count * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription(_indexCount * sizeof(ushort), BufferUsage.IndexBuffer));

        _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertexArray);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, m.Indices);

        var vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float3), new VertexElementDescription("Texture Coordinate",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float2));

        var vertexCode = File.ReadAllText("Shader/shader.vert").ReplaceLineEndings();
        var fragmentCode= File.ReadAllText("Shader/shader.frag").ReplaceLineEndings();

        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentCode), "main");
        
        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        var projViewLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        var worldLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
        
        var pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        pipelineDescription.ResourceLayouts = [];
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, _shaders);
        pipelineDescription.ResourceLayouts = new[] { projViewLayout, worldLayout };
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
            projViewLayout, _projectionBuffer, _viewBuffer
        ));
        
        _worldSet = factory.CreateResourceSet(new ResourceSetDescription(worldLayout, _worldBuffer
        ));

        _commandList = factory.CreateCommandList();
        
    }

    private static void Draw()
    {
        _ticks += TimeHandler.DeltaTime * 1000f;
        _commandList.Begin();
      
        var projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(
                1, 
                (float)(_window.Width)/_window.Height,
                0.001f,
                1000000000f);
        _commandList.UpdateBuffer(_projectionBuffer, 0, projectionMatrix);
        
        _commandList.UpdateBuffer(_viewBuffer, 0, _mainCamera.ViewMatrix);
        // _commandList.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(_mainCamera.Position, Vector3.Zero, Vector3.UnitY));
        Matrix4x4 rotation =
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (_ticks / 1000f))
            * Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, 0);
        _commandList.UpdateBuffer(_worldBuffer, 0, ref rotation); 
        
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);
        _commandList.SetPipeline(_pipeline);
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        _commandList.SetGraphicsResourceSet(0, _projViewSet);
        _commandList.SetGraphicsResourceSet(1, _worldSet);

        _commandList.DrawIndexed(_indexCount, 1, 0, 0, 0);
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
        _graphicsDevice.WaitForIdle();
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

struct VertexPositionTexture
{
    public const uint SizeInBytes = 20;
    public float PosX;
    public float PosY;
    public float PosZ;

    public float TexU;
    public float TexV;

    public VertexPositionTexture(Vector3 pos, Vector2 uv)
    {
        PosX = pos.X;
        PosY = pos.Y;
        PosZ = pos.Z;
        TexU = uv.X;
        TexV = uv.Y;
    }
}