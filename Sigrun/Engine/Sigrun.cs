using System.Net.Mime;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Logging;
using Sigrun.Player;
using Sigrun.Player.Components;
using Sigrun.Rendering;
using Sigrun.Rendering.Entities;
using Sigrun.Rendering.Loader;
using Sigrun.Time;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Vortice.Direct3D11;
using BufferDescription = Veldrid.BufferDescription;

namespace Sigrun.Engine;

static class Sigrun
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
    private static ResourceSet _textureSet;
    
    private static uint _indexCount;

    private static ImGuiRenderer _imGuiRenderer;
    
    private static Shader[] _shaders;
    private static Pipeline _pipeline;

    private static List<Renderer> _renderables = [];

    private static float _ticks;
    private static InputSnapshot _inputSnapshot;
    
    private static Sdl2Window _window;
    
    private static Camera _mainCamera;

    private static DateTime _lastFixedUpdate = DateTime.Now;
    private static float _fixedUpdateMillis = 10;

    private static Player.Player _player = new ();

    private static ILogger _logger = LoggingProvider.NewLogger("Sigrun.Engine.Sigrun");

    
    private static List<GameObject> _gameObjects = [];
    
    public static void Start()
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

        _window.KeyDown += InputState.OnKeyDown;
        _window.KeyUp += InputState.OnKeyUp;
        _window.MouseMove += InputState.OnMouseMove;
       
        _window.FocusGained += OnFocusGained;
        _window.FocusLost += OnFocusLost;

        _window.Resized += () =>
        {
            _imGuiRenderer.WindowResized(_window.Width, _window.Height);
            _graphicsDevice.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
        };


        _mainCamera = _player.Camera;
        
        SpawnObject(_player);
        

        var obj1 = GameObject.FromModelFile("Assets/Models/4tunnels.rmesh", "4tunnel");
        var obj2 = GameObject.FromModelFile("Assets/Models/173.rmesh", "173");
        obj2.Scale = 0.005f;
        obj1.Scale = 0.005f;
        obj2.Position -= Vector3.UnitY * 25f;
        SpawnObject(obj1);
        SpawnObject(obj2);

        DateTime timer;
        while (_window.Exists)
        {
            timer = DateTime.Now;
            TimeHandler.UpdateDeltaTime();
            _inputSnapshot = _window.PumpEvents();
            Update();
            if ((timer - _lastFixedUpdate).Milliseconds >= _fixedUpdateMillis)
            {
                _lastFixedUpdate = DateTime.Now;
                FixedUpdate();
            }
            Draw();
        }
    }

    public static void SpawnObject(GameObject obj)
    {
        _gameObjects.Add(obj);
        if (obj.Components == null) return;
        foreach (var objComponent in obj.Components)
        {
           objComponent.Startup(); 
        }
    }
    
    private static void Update()
    {
        foreach (var obj in _gameObjects)
        {
            foreach (var component in obj.Components)
            {
                switch (component)
                {
                    case InputHandler inputHandler:
                        inputHandler.Snapshot = _inputSnapshot;
                        break;
                    default:
                        component.Update();
                        break;
                }
            }
        }
    }

    private static void FixedUpdate()
    {
        foreach (var obj in _gameObjects)
        {
            foreach (var component in obj.Components)
            {
                switch (component)
                {
                    case InputHandler inputHandler:
                        inputHandler.Snapshot = _inputSnapshot;
                        break;
                    default:
                        component.FixedUpdate();
                        break;
                }
            }
        }
    }

    private static void ObjectStartup()
    {
        foreach (var obj in _gameObjects)
        {
            if (obj.Components == null) continue;
            foreach (var comp in obj.Components)
            {
                comp.Startup();
            }
        } 
    }

    public static void AddModelToRender(Renderer model)
    {
        _renderables.Add(model);
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
        
        
        var image = new ImageSharpTexture("Assets/Textures/missingTexture.jpg");

        var tex = image.CreateDeviceTexture(_graphicsDevice, factory);
        var view = factory.CreateTextureView(tex);

        var worldTextureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _textureSet =
            factory.CreateResourceSet(new ResourceSetDescription(worldTextureLayout, view,
                _graphicsDevice.Aniso4xSampler));
        
        var pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        pipelineDescription.ResourceLayouts = [];
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, _shaders);
        pipelineDescription.ResourceLayouts = new[] { projViewLayout, worldLayout, worldTextureLayout };
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
        _commandList.SetGraphicsResourceSet(2, _textureSet);
        _imGuiRenderer.Update(TimeHandler.DeltaTime, _inputSnapshot);

        ImGui.Begin("Information");
        ImGui.Text($"FPS: {TimeHandler.FramesPerSecond}");
        ImGui.Text($"FrameTime: {TimeHandler.FrameTime}ms");
        ImGui.Text($"Camera Pos: {_mainCamera.Position.X} {_mainCamera.Position.Y} {_mainCamera.Position.Z}");
        ImGui.Text($"Num of Meshes to render: {_renderables.Count}");
        
        DrawObjects();
        
        _imGuiRenderer.Render(_graphicsDevice, _commandList);
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
        _graphicsDevice.WaitForIdle();
    }

    public static void DrawObjects()
    {
        var factory = _graphicsDevice.ResourceFactory;
        foreach (var renderer in _renderables)
        {
            DrawObject(factory, renderer.Model.Mesh, renderer.Parent);
            foreach (var entity in renderer.Model.Entities)
            {
                if (entity is not ModelEntity modelEntity || modelEntity.Mesh == null) continue;
                DrawObject(factory, modelEntity.Mesh, renderer.Parent);
            }
        }
    }

    private static void DrawObject(ResourceFactory factory, Mesh mesh, GameObject obj)
    {
        // Object translation
        var objectData = new GPUModel();
        objectData.ModelMatrix = Matrix4x4.CreateTranslation(obj.Position);
        _commandList.UpdateBuffer(_worldBuffer, 0, ref objectData); 

        // Upload model to GPU for rendering 
        _vertexBuffer =
            factory.CreateBuffer(new BufferDescription((uint)mesh.Vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)mesh.Indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));

        var vertexArray = new VertexPositionTexture[mesh.Vertices.Length];
        for (int j = 0; j < vertexArray.Length; j++)
        {
            var vert = mesh.Vertices[j];
            vertexArray[j] = new VertexPositionTexture(vert.Position * obj.Scale, vert.Uv, vert.TextureIndex);
        }
            
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertexArray);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, mesh.Indices);
            
            
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            

        
        // Draw model
        _commandList.DrawIndexed((uint)mesh.Indices.Length);
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
    
    public static void Dispose()
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