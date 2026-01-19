using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Sigrun.Rendering;

namespace Sigrun.Engine.Entity.Components;

public class Renderer : Component
{
    public Renderer(GameObject parent, Model model) : base(parent)
    {
        Model = model;
    }

    public Model Model { get; set; }

    public override void Startup()
    {
        Sigrun.AddModelToRender(this);
    }
}