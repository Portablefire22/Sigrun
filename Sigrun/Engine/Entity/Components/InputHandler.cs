using Veldrid;
namespace Sigrun.Engine.Entity.Components;

/// <summary>
/// Is provided with an input snapshot before every Update() and FixedUpdate(),
/// allowing for input not handled by InputState to be processed by the component.
/// </summary>
public class InputHandler : Component
{
    public InputSnapshot Snapshot { private get; set; }
    public InputHandler(GameObject parent) : base(parent)
    {
    }
}