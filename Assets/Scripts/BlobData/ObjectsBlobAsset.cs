using Unity.Entities;

public struct ObjectsBlobAsset
{
    public BlobArray<InteractableObjectData> Array;
}

public struct ObjectsBlobData: IComponentData
{
    public InteractableObjectData Data;
}