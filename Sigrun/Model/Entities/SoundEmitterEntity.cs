using System.Numerics;

namespace Sigrun.Model.Entities;

public class SoundEmitterEntity : RoomMeshEntity
{
    public int SoundIndex { get; set; }
    public float Range { get; set; }
    
    public SoundEmitterEntity(Vector3 position,int soundIndex, float range) : base(position)
    {
        SoundIndex = soundIndex;
        Range = range;
    }
}