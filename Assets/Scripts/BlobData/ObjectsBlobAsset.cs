using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ObjectsBlobAsset
{
    public BlobArray<ObjectsBlobData> Array;
    public BlobArray<NeedsData> NeedsData;
}

public struct NeedsData: IComponentData
{
    public BlobArray<float> Curve;
    public float3 MinValue;
    public float3 ZeroValue;
    public float3 MaxValue;
    public float3 DecayRate;
    public ENeed Type;
}

public struct ObjectsBlobData: IComponentData
{
    public FixedString32Bytes ExampleData;
}