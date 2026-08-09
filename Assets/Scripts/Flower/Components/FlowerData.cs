
using System;
using Unity.Entities;
using Unity.Mathematics;
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
    public float3 position;

    public Entity owner;
    public float nectar;

}