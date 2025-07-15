using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Data/Object")]
public class ObjectData: ScriptableObject
{
    public Need[] NeedsAdvertised;
}