using Unity.Entities;
using Unity.Mathematics;

public struct RandomSingleton: IComponentData
{
    public Random Random;
}