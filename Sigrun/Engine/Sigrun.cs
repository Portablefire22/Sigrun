using System.Numerics;
using System.Text;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Entity.Components.Physics;
using Sigrun.Engine.Entity.Components.Physics.Colliders;
using Sigrun.Engine.Logging;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Entities;
using Sigrun.Engine.Rendering.Primitives;
using Sigrun.Engine.Time;
using Sigrun.Game.Player;
using Sigrun.Game.Player.Components;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
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
    private static TextureView _textureView;
    private static ResourceSet _projViewSet;
    private static ResourceSet _worldSet;
    private static ResourceSet _objectInfoSet;
    private static ResourceSet _textureSet;

    private static List<Mesh> _alphaMeshes = [];
    
    private static uint _indexCount;

    private static ImGuiRenderer _imGuiRenderer;
    
    private static Shader[] _shaders;
    private static Pipeline _pipeline;
    private static Pipeline _alphaPipeline;

    private static List<Renderer> _renderables = [];

    private static float _ticks;
    private static InputSnapshot _inputSnapshot;
    
    private static Sdl2Window _window;

    private static bool _mouseCaptured = true;
    
    private static Camera _mainCamera;

    private static DateTime _lastFixedUpdate = DateTime.Now;
    private static float _fixedUpdateMillis = 20;

    private static Player _player = new ();

    private static ILogger _logger = LoggingProvider.NewLogger("Sigrun.Engine.Sigrun");

    
    private static List<GameObject> _gameObjects = [];
    private static List<Collider> _colliders = [];
    
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
        _window.KeyDown += GlobalKeys;
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

        // var obj1 = GameObject.FromModelFile("Assets/Models/4tunnels.rmesh", "4tunnel");
        // var obj2 = GameObject.FromModelFile("Assets/Models/173.rmesh", "173");
        // obj2.Scale = 0.005f;
        // obj1.Scale = 0.0005f;
        // obj2.Position -= Vector3.UnitY * 25f;
        //
        // var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
        // obj2.Components.Add(rigidbody);
        
        var obj2 = new GameObject();
        
        var mod = new Model() { Meshes = [new CubeMesh(new Vector3(2))] };
        var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
        var renderer = new Renderer(obj2, mod);
        obj2.Components.Add(renderer);
        obj2.Components.Add(rigidbody);
        obj2.Position += new Vector3(3, 0, 0);

        var obj3 = new GameObject();
        
        var mod2 = new Model() { Meshes = [new CubeMesh(new Vector3(2))] };
        var rigidbody2 = new Rigidbody(obj2) {Collider = new BoxCollider(obj3) };
        var renderer2 = new Renderer(obj3, mod2);
        obj3.Components.Add(renderer2);
        obj3.Components.Add(rigidbody2);

        rigidbody.Collider.Intersects(rigidbody2.Collider);
        
        SpawnObject(obj2);
        SpawnObject(obj3);

        Sdl2Native.SDL_SetHint("SDL_MOUSE_RELATIVE_MODE_CENTER", "1");
        
        DateTime timer;
        while (_window.Exists)
        {
            timer = DateTime.Now;
            TimeHandler.UpdateDeltaTime();
            _inputSnapshot = _window.PumpEvents();
            InputState.Delta = _window.MouseDelta;
            Update();
            var diff = (timer - _lastFixedUpdate).Milliseconds;
            while (diff >= _fixedUpdateMillis)
            {
                _lastFixedUpdate = DateTime.Now;
                FixedUpdate();
                diff -= 20;
            }
            TextureHandler.CreateSets(_graphicsDevice);
            Draw();
        }
    }

    public static void GlobalKeys(KeyEvent e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                _mouseCaptured = !_mouseCaptured;
                CaptureMouse(_mouseCaptured);
                break;
        } 
    }

    public static void CaptureMouse(bool shouldCapture)
    {
        Sdl2Native.SDL_SetRelativeMouseMode(shouldCapture);
    }

    public static void SpawnObject(GameObject obj)
    {
        _gameObjects.Add(obj);
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
        // Physics
        for (int i = 0; i < _colliders.Count / 2; i++)
        {
            for (int j = _colliders.Count; j > i; j++)
            {
                var col1 = _colliders[i];
                var col2 = _colliders[j];
                
                if (!col1.Intersects(col2)) continue;

                col1.Touching.Add(col2);
                col2.Touching.Add(col1);
            }
        }
        
        // Component FixedUpdate
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
        CaptureMouse(false);
    }

    private static void OnFocusGained()
    {
        CaptureMouse(true);
    }

    public static void AddCollider(Collider collider)
    {
        
    }


    private static void CreateResources()
    {
        var factory = _graphicsDevice.ResourceFactory;

        _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        var vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float3), new VertexElementDescription("Texture Coordinate",
            VertexElementSemantic.TextureCoordinate,
            VertexElementFormat.Float2), 
            new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Alpha", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1));

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


        // _textureBuffer = factory.CreateBuffer(new BufferDescription((uint) (1.18 * Math.Pow(10, 8)), BufferUsage.UniformBuffer));

        TextureHandler.AddTexture("missingTexture.jpg");

        
        var worldTextureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTextures", ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        var pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        pipelineDescription.ResourceLayouts = [];
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, _shaders);
        pipelineDescription.ResourceLayouts = new[] { projViewLayout, worldLayout, worldTextureLayout };
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        
        pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        pipelineDescription.ResourceLayouts = [];
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, _shaders);
        pipelineDescription.ResourceLayouts = new[] { projViewLayout, worldLayout, worldTextureLayout };
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
        _alphaPipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        
        
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
            foreach (var mesh in renderer.Model.Meshes)
            {
                if (mesh.Alpha)
                {
                    _alphaMeshes.Add(mesh);
                    continue;
                }
                DrawObject(factory, mesh, renderer.Parent);
            }

            if (renderer.Model.Entities != null)
            {
                foreach (var entity in renderer.Model.Entities)
                {
                    if (entity is not ModelEntity modelEntity || modelEntity.Mesh == null) continue;

                    if (modelEntity.Mesh.Alpha)
                    {
                        _alphaMeshes.Add(modelEntity.Mesh);
                        continue;
                    }

                    DrawObject(factory, modelEntity.Mesh, renderer.Parent);
                }
            }

            // For objects that contain transparency we switch to a pipeline that does not cull backfaces
            // This prevents the back of glass from being culled, with the added benefit that glass does 
            // not prevent other faces from rendering
            if (_alphaMeshes.Count == 0) continue;
            _commandList.SetPipeline(_alphaPipeline);
            foreach (var mesh in _alphaMeshes)
            {
                DrawObject(factory, mesh, renderer.Parent);
            }
            _alphaMeshes.Clear();
            _commandList.SetPipeline(_pipeline);
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
            vertexArray[j] = new VertexPositionTexture(vert.Position * obj.Scale, vert.Uv, vert.Normal, vert.Alpha);
        }
            
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertexArray);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, mesh.Indices);
            
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            
        // _textureView = TextureHandler.GetTextureView(_graphicsDevice, _graphicsDevice.ResourceFactory, mesh.Textures);
        //         
        // var worldTextureLayout = factory.CreateResourceLayout(
        //     new ResourceLayoutDescription(
        //         new ResourceLayoutElementDescription("SurfaceTextures", ResourceKind.TextureReadOnly,
        //             ShaderStages.Fragment),
        //         new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        // _textureSet =
        //     factory.CreateResourceSet(new ResourceSetDescription(worldTextureLayout, _textureView,
        //         _graphicsDevice.Aniso4xSampler));
        // _commandList.SetGraphicsResourceSet(2, _textureSet);

        _textureSet = TextureHandler.GetTextureSet(mesh.Texture);
        
        _commandList.SetGraphicsResourceSet(2, _textureSet);

        
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
        TextureHandler.Dispose();
        _graphicsDevice.Dispose();
    }
}

struct VertexPositionTexture
{
    public const uint SizeInBytes = 36;
    public float PosX;
    public float PosY;
    public float PosZ;

    public float TexU;
    public float TexV;

    public float NormalX;
    public float NormalY;
    public float NormalZ;
    
    public float Alpha;
    


    public VertexPositionTexture(Vector3 pos, Vector2 uv, Vector3 normal, float alpha)
    {
        PosX = pos.X;
        PosY = pos.Y;
        PosZ = pos.Z;
        TexU = uv.X;
        TexV = uv.Y;
        NormalX = normal.X;
        NormalY = normal.Y;
        NormalZ = normal.Z;
        Alpha = alpha;
    }
}