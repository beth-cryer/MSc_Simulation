using Unity.Entities;

public struct ActionSetNeed : IComponentData
{
    public ENeed Need;
    public EActionType Type;
    public float Value;
    public float Time;
}

public enum EActionType
{
    SetNeed,    // (only set value at end of action) eg. social actions
    ChangeNeed, // (change value as action is performed) eg. sleeping, eating

}