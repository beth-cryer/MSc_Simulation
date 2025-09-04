using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ObjectsBlobAsset
{
    public BlobArray<ObjectsBlobData> Array;
    public BlobArray<NeedsData> NeedsData;
	public BlobArray<TraitsData> TraitsData;
	public BlobArray<EmotionsData> EmotionsData;
    public DistanceScalingData DistanceScalingData;
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

public struct TraitsData: IComponentData
{
	public Trait Trait;
}

public struct EmotionsData: IComponentData
{
	public EEmotion Type;
	public float3 PADValue;
}

public struct DistanceScalingData
{
    public BlobArray<float> DistanceCurve;
    public float MinDistance;
    public float MaxDistance;
}

public struct ObjectsBlobData: IComponentData
{
    public FixedString32Bytes ExampleData;
}