using System.Numerics;

namespace Sigrun.Rendering.Loader;

public class ObjLoader
{
    private FileStream _inputFile;

    private uint _vertCount;
    private uint _texCount;
    private uint _normalCount;
    private uint _spaceCount;

    private string _name;
    
    public Mesh LoadFromFile(string path)
    {
        _inputFile = new FileStream(path, FileMode.Open);
        var reader = new StreamReader(_inputFile);
        var line = reader.ReadLine();

        var verts = new List<MeshVertex>();
        var ind = new List<ushort>();

        float p1, p2, p3;
        
        while (line != null)
        {
            var splits = line.Split(" ");
            switch (splits[0])
            {
                case "v":
                    p1 = float.Parse(splits[1]);
                    p2 = float.Parse(splits[2]);
                    p3 = float.Parse(splits[3]);

                    var vert = new MeshVertex()
                    {
                        Position =  new Vector3(p1,p2,p3),
                    };
                    verts.Add(vert);
                    break;
                case "vn":
                    p1 = float.Parse(splits[1]);
                    p2 = float.Parse(splits[2]);
                    verts[(int)_texCount].Uv = new Vector2(p1, p2);
                    _texCount++;
                    break;
                case "vt":
                    break;
                case "vp": 
                    break;
                case "f":
                    foreach (var point in splits[1..])
                    {
                        var tmp = point.Split("/");
                        var index = ushort.Parse(tmp[0]);
                        ind.Add((ushort)(index - 1));
                    }
                    break;
                case "usemtl":
                    break;
                case "o":
                    _name = splits[1];
                    break;
                case "#":
                    break;
                case "mtllib": break;
                case "g": break;
                default:
                    throw new ArgumentException($"Unknown OBJ data type {splits[0]}");
            }
            
            line = reader.ReadLine();
        }

        return new Mesh()
        {
            Vertices = verts.ToArray(),
            Indices = ind.ToArray(),
            Name = _name,
            Texture = ""
        };
    }
}