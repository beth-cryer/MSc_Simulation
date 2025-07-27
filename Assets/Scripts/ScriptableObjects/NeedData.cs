using UnityEngine;

[CreateAssetMenu(fileName = "Need", menuName = "Data/Need")]
public class NeedData : ScriptableObject
{
    public string Name;
    public ENeed Type;
    public bool IsVisibleNeed = true;
    public AnimationCurve Curve;
}