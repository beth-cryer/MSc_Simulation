using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Data/Object")]
public class ObjectData: ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public NeedAdvertisedData[] NeedsAdvertised;
    public float InteractDuration = 5.0f;
}

[Serializable]
public class NeedAdvertisedData
{
    public Need NeedAdvertised;
    public EActionType ActionType;
    public float3 MoveTowardsAmount = 0.0f;
}