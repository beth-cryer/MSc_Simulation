using Unity.Collections;
using Unity.Entities;

struct InteractableObject: IComponentData
{
    public FixedString32Bytes Name;
    public float InteractDuration;
}