using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;

namespace Sigrun.Game.World;

public class ScpRoom : Component
{
    public ScpRoom(GameObject parent) : base(parent)
    {
    }

    public RoomType Shape { get; set; }
    public string Name { get; set; }
    public string MeshPath { get; set; }
    public GameObject DoorOne { get; set; }
    public GameObject DoorTwo { get; set; }
    public int Zone { get; set; }
}