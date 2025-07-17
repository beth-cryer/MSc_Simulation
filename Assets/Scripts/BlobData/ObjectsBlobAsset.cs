using Unity.Collections;
using Unity.Entities;

public struct ObjectsBlobAsset
{
    public BlobArray<ObjectsBlobData> Array;
    public BlobArray<NeedsData> NeedsData;
}

public struct NeedsData: IComponentData
{
    public ENeed Type;
    public BlobArray<float> Curve;
}

public struct ObjectsBlobData: IComponentData
{
    public FixedString64Bytes ExampleData;
}