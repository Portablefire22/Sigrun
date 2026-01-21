using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using Sigrun.Engine;
using Sigrun.Logging;
using Sigrun.Rendering.Entities;

namespace Sigrun.Rendering.Loader;

public class RMeshLoader2
{
    
private FileStream _fileStream;

    private bool _hasTriggers;
    private int _textureCount;
    private int _indicesOffset;
    private List<MeshVertex[]> _textureVertices;
    private List<ushort[]> _textureIndices = [];
    private List<string> _texturePaths;
    private List<ushort> _vertexIndices;

    private string _name;
    private int _vertexCount;

    private ILogger _logger;

    public RMeshLoader2()
    {
        _logger = LoggingProvider.NewLogger<RMeshLoader2>();
    }

    // This holds the traditional entities but also the different meshes
    private List<RoomMeshEntity> _entities;

    public Rendering.Model LoadFromFile(string path, string name)
    {
        _hasTriggers = false;
        _textureCount = 0;
        _indicesOffset = 0;
        _textureVertices = new List<MeshVertex[]>();
        _texturePaths = new List<string>();
        _vertexIndices = new List<ushort>();
        _entities = new List<RoomMeshEntity>();
        _vertexCount = 0;
        _name = name;

        _fileStream = new FileStream(path, FileMode.Open);
        return Read();
    }

    private Rendering.Model Read()
    {
        switch (ReadB3DString())
        {
            case "RoomMesh":
                break;
            case "RoomMesh.HasTriggerBox":
                _hasTriggers = true;
                break;
            default:
                throw new Exception("Not an rmesh file");
        }

        var meshes = new List<Mesh>();
        _textureCount = ReadInt32();
        for (int i = 0; i < _textureCount; i++)
        {
            ReadTexture();
        }
        GetInvisCollisions();
        if (_hasTriggers) GetTriggerBoxes();
        GetEntities();
        _fileStream.Close();

        for (int j = 0; j < _texturePaths.Count; j++)
        {
            var mesh = new Mesh()
            {
                Indices = _textureIndices[j],
                Name = $"{_name}_{_texturePaths[j]}",
                Texture = _texturePaths[j],
                Vertices = _textureVertices[j],
                Alpha = _texturePaths[j].Contains("glass")
            };
            meshes.Add(mesh);
        }
        
        return new Rendering.Model()
        {
            Entities = _entities.ToArray(),
            Meshes = meshes.ToArray()
        };
    }

    private int ReadInt32()
    {
        byte[] buf = new byte[4];
        _fileStream.ReadExactly(buf, 0, 4);
        return BitConverter.ToInt32(buf);
    }

    private string ReadB3DString()
    {
        var length = ReadInt32();
        var buf = new byte[length];
        _fileStream.ReadExactly(buf, 0, length);
        return Encoding.UTF8.GetString(buf);
    }

    private float ReadFloat32()
    {
        byte[] buf = new byte[4];
        _fileStream.ReadExactly(buf, 0,4);
        return BitConverter.ToSingle(buf);
    }

    private Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat32(), ReadFloat32(), ReadFloat32());
    }
    
    private Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat32(), ReadFloat32());
    }

    private MeshVertex ReadVertexData()
    {
        var position = ReadVector3();
        var uv = ReadVector2();
        var lightmapUv = ReadVector2();
        var buf = new byte[3];
        _fileStream.ReadExactly(buf, 0,3);

        return new MeshVertex()
        {
            Position = position with { Z = -position.Z },
            Uv = uv,
            LightmapUv = lightmapUv,
            Red = buf[0],
            Green = buf[1],
            Blue = buf[2],
            Index = _vertexCount++
        };
    }

    public void ReadOpaque()
    {
        var relativePath = ReadB3DString();
        _texturePaths.Add(relativePath);
        ReadTextureObjectData(relativePath);
    }

    public void ReadLightmap()
    {
        var relativePath = ReadB3DString();
    }

    public void ReadTransparency()
    {
        var relativePath = ReadB3DString();
        _texturePaths.Add(relativePath);
        ReadTextureObjectData(relativePath);
    }
    
    private bool ReadTextureObjectData(string relativePath)
    {
        var vertexCount = ReadInt32();
        TextureHandler.AddTexture(relativePath);

        var alpha = relativePath.Contains("glass");
        
        var vertices = new MeshVertex[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            var v = ReadVertexData();
            v.Alpha = alpha ? 0.5f : 1f;
            vertices[i] = v;
        }
        _textureVertices.Add(vertices); 
        var triangleCount = ReadInt32();
        var index = new ushort[triangleCount * 3];
        for (int i = 0; i < triangleCount*3; i++)
        {
            var localIndex = ReadInt32();
            index[i] = (ushort)(localIndex);
        }
        _textureIndices.Add(index);
        _indicesOffset += vertexCount;
        return alpha;
    }

    public void ReadTexture()
    {
        var flag = _fileStream.ReadByte();
        ReadLightmap();
        flag = _fileStream.ReadByte();
        switch (flag)
        {
            case 1:
                ReadOpaque();
                break;
            case 3:
                ReadTransparency();
                break;
            default:
                throw new ArgumentException($"invalid flag '{flag}'");
        }
    }
    
    public void GetInvisCollisions()
    {
        var invisCollisions = ReadInt32();
        for (int i = 0; i < invisCollisions; i++)
        {
            if (invisCollisions == 0) return;
            var invisCollisionsVertices = ReadInt32();
            for (int j = 0; j < invisCollisionsVertices; j++)
            {
                var vert = new InvisibleCollisionVertex(ReadVector3());
            }

            var invisCollisionsTriangles = ReadInt32();
            for (int j = 0; j < invisCollisionsTriangles * 3; j++)
            {
                ReadInt32(); // index
            }
        }
    }

    public void GetTriggerBoxes()
    {
        var count = ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var surfaceAmount = ReadInt32();
            var vertexCount = ReadInt32();
            for (int j = 0; j < vertexCount; j++)
            {
                var vert = new InvisibleCollisionVertex(ReadVector3());
            }
           

            var triangleCount = ReadInt32(); 
            for (int j = 0; j < triangleCount * 3; j++)
            {
                // _logger.LogInformation("Index: {}, Written: {}, Offset: {}", i, _ind, _indicesOffset);
                // indices[i] =  _indicesOffset + ReadInt32();
                ReadInt32();
                // _ind++;
            }
            var triggerBoxName = ReadB3DString();
        }
    }
    
    public void GetEntities()
    {
        var entityCount = ReadInt32();
        for (int i = 0; i < entityCount; i++)
        {
            var type = ReadB3DString();
            switch (type)
            {
                case "screen":
                    var s = ReadScreen();
                    _entities.Add(s);
                    break;
                case "waypoint":
                    var w = ReadWaypoint();
                    _entities.Add(w);
                    break;
                case "light":
                    var l = ReadLight();
                    _entities.Add(l);
                    break;
                case "spotlight":
                    var sl = ReadSpotlight();
                    _entities.Add(sl);
                    break;
                case "soundemitter":
                    var se = ReadSoundEmitter();
                    _entities.Add(se);
                    break;
                case "playerstart":
                    var ps = ReadPlayerStart();
                    _entities.Add(ps);
                    break;
                case "model":
                    var m = ReadModel();
                    _entities.Add(m);
                    break;
                default:
                    throw new Exception("invalid entity type");
            }
        }
    }

    private ScreenEntity ReadScreen()
    {
        return new ScreenEntity(ReadVector3(), ReadB3DString());
    }

    private WaypointEntity ReadWaypoint()
    {
        return new WaypointEntity(ReadVector3());
    }

    private LightEntity ReadLight()
    {
        return new LightEntity(ReadVector3(), ReadFloat32(), ReadB3DString(), ReadFloat32());
    }

    private SpotlightEntity ReadSpotlight()
    {
        return new SpotlightEntity(ReadVector3(), ReadFloat32(), ReadB3DString(), ReadFloat32(), 
            ReadB3DString(), ReadInt32(), ReadInt32());
    }

    private SoundEmitterEntity
        ReadSoundEmitter()
    {
        return new SoundEmitterEntity(ReadVector3(), ReadInt32(), ReadFloat32());
    }

    private PlayerStartEntity ReadPlayerStart()
    {
        return new PlayerStartEntity(ReadVector3(), ReadB3DString());
    }

    private ModelEntity ReadModel()
    {
        return new ModelEntity(ReadB3DString(), ReadVector3(), ReadVector3(), ReadVector3());
    }
    
    public class InvisibleCollisionVertex
    {
    
        public Vector3 Position { get; set; }
    
        public InvisibleCollisionVertex(Vector3 position)
        {
            Position = position;
        }
    }}