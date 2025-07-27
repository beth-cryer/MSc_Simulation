using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Data/Object")]
public class ObjectData: ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public Need[] NeedsAdvertised;
}