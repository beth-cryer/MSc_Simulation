using Unity.Entities;

struct Location: IComponentData
{
    public Entity ParentLocation;
    public DynamicBuffer<Entity> ChildLocations;
}