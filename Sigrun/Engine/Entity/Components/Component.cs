namespace Sigrun.Engine.Entity.Components;

/// <summary>
/// Every object with a function is composed of components, every component is called
/// every frame with Update, on a fixed interval with FixedUpdate(), and on creation
/// with Startup()
/// </summary>
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

    protected void SpawnObject(GameObject gameObject)
    {
        Sigrun.SpawnObject(gameObject);
    }
    
    public GameObject Parent { get; private set; }
}