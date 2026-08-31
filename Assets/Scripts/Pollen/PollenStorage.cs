
using Unity.Entities;

public struct PollenStorage : IBufferElementData
{
    public FlowerType Type;
    public float Amount;
}