namespace Sigrun.Engine.Entity.Components;

public abstract class Component
{
    protected Component(GameObject parent)
    {
        Parent = parent;
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    public virtual void Update(){}
    /// <summary>
    /// Called on a minimum fixed-interval.
    /// </summary>
    public virtual void FixedUpdate(){}
    /// <summary>
    /// Called as soon as the component is created.
    /// </summary>
    public virtual void Startup(){}
    
    public GameObject Parent { get; private set; }
}