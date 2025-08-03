using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Need", menuName = "Data/Need")]
public class NeedData : ScriptableObject
{
    public string Name;
    public ENeed Type;
    public float3 MinValue = 0.0f;
    public float3 ZeroValue = 0.0f;
    public float3 MaxValue = 100.0f;
    public float3 DecayRate = 60.0f / 120.0f;
    public bool IsVisibleNeed = true;
    public AnimationCurve Curve;
}