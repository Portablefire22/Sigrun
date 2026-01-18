using System.Numerics;
using System.Text;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Sigrun.Model;
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
    private static DeviceBuffer _objectInfoBuffer;
    private static ResourceSet _projViewSet;
    private static ResourceSet _worldSet;
    private static ResourceSet _objectInfoSet;
    
    private static uint _indexCount;

    private static ImGuiRenderer _imGuiRenderer;
    
    private static Shader[] _shaders;
    private static Pipeline _pipeline;

    private static List<Model.Model> _models;

    private static float _ticks;
    private static InputSnapshot _inputSnapshot;
    
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

        _window.Resized += () =>
        {
            _imGuiRenderer.WindowResized(_window.Width, _window.Height);
            _graphicsDevice.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
        };
        
        
        while (_window.Exists)
        {
            TimeHandler.UpdateDeltaTime();
            _inputSnapshot = _window.PumpEvents();
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

        var loader = new RMeshLoader();
        var m = loader.LoadFromFile(
            "Assets/Models/173.rmesh", "008");
        _models = new List<Model.Model>();
        _models.Add(m); 
        m = loader.LoadFromFile(
            "Assets/Models/4tunnels.rmesh", "008");
        m.Position -= Vector3.UnitY * 20f;
        _models.Add(m);
        _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        var vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float3), new VertexElementDescription("Texture Coordinate",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float2), new VertexElementDescription("Texture Index", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1));

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

        _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _graphicsDevice.SwapchainFramebuffer.OutputDescription, 200, 100);
        _imGuiRenderer.WindowResized(_window.Width, _window.Height);
        _commandList = factory.CreateCommandList();
        
    }

    private static void Draw()
    {
        _ticks += TimeHandler.DeltaTime * 1000f;
        _commandList.Begin();
      
       
        _commandList.UpdateBuffer(_projectionBuffer, 0, 
            _mainCamera.GetProjectionMatrix((float)_window.Width/_window.Height));
        
        _commandList.UpdateBuffer(_viewBuffer, 0, _mainCamera.ViewMatrix);
        
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);
        _commandList.SetPipeline(_pipeline);

        _commandList.SetGraphicsResourceSet(0, _projViewSet);
        _commandList.SetGraphicsResourceSet(1, _worldSet);
        _imGuiRenderer.Update(TimeHandler.DeltaTime, _inputSnapshot);

        ImGui.Begin("Information");
        ImGui.Text($"FPS: {TimeHandler.FramesPerSecond}");
        ImGui.Text($"FrameTime: {TimeHandler.FrameTime}ms");
        ImGui.Text($"Camera Pos: {_mainCamera.Position.X} {_mainCamera.Position.Y} {_mainCamera.Position.Z}");
        ImGui.Text($"Num of Meshes to render: {_models.Count}");
        
        DrawObjects();
        
        _imGuiRenderer.Render(_graphicsDevice, _commandList);
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
        _graphicsDevice.WaitForIdle();
    }

    public static void DrawObjects()
    {
        var i = 0;
        var factory = _graphicsDevice.ResourceFactory;
        foreach (var model in _models)
        {
            var objectData = new GPUModel();
            model.Position ??= new Vector3(0);
            model.Scale = 0.005f;
            
            objectData.ModelMatrix = Matrix4x4.CreateTranslation((Vector3)model.Position);

            _commandList.UpdateBuffer(_worldBuffer, 0, ref objectData); 

            _vertexBuffer =
                factory.CreateBuffer(new BufferDescription((uint)model.Mesh.Vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)model.Mesh.Indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));

            var vertexArray = new VertexPositionTexture[model.Mesh.Vertices.Length];
            for (int j = 0; j < vertexArray.Length; j++)
            {
                var vert = model.Mesh.Vertices[j];
                vertexArray[j] = new VertexPositionTexture(vert.Position * model.Scale, new Vector2(), vert.TextureIndex);
            }
            
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertexArray);
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, model.Mesh.Indices);
            
            
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            
            _commandList.DrawIndexed((uint)model.Mesh.Indices.Length);
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            i++;
        }
    }
    
    private static void Dispose()
    {
        _pipeline.Dispose();
        _commandList.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _imGuiRenderer.Dispose();
        _graphicsDevice.Dispose();
    }
}

struct VertexPositionTexture
{
    public const uint SizeInBytes = 24;
    public float PosX;
    public float PosY;
    public float PosZ;

    public float TexU;
    public float TexV;

    public int TexIndex;

    public VertexPositionTexture(Vector3 pos, Vector2 uv, int texIndex)
    {
        PosX = pos.X;
        PosY = pos.Y;
        PosZ = pos.Z;
        TexU = uv.X;
        TexV = uv.Y;
        TexIndex = texIndex;
    }
}