using UnityEngine;

[CreateAssetMenu(fileName = "Need", menuName = "Data/Need")]
public class NeedData : ScriptableObject
{
    public ENeed Type;
    public AnimationCurve Curve;
}