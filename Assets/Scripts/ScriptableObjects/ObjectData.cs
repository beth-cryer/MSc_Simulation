using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Data/Object")]
public class ObjectData: ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public NeedAdvertisedData[] NeedsAdvertised;
}

[Serializable]
public class NeedAdvertisedData
{
    public Need NeedAdvertised;
    public EActionType ActionType;
    public float3 NeedValueChange = 0.0f;
    public float InteractDuration = 5.0f;
    public float MinInteractDuration = 5.0f;
    public bool RequiredToCompleteAction = true; // Determines if this Need has to be completed in order for the Action to be completed
}