using Unity.Entities;

public enum FlowerType
{
    Dandelion,
    Tulip,
    Sunflower
}

public struct FlowerData : IComponentData, IEnableableComponent
{
    public float pollen;
    public FlowerType type;
    public Entity owner;
    public float nectar;
}