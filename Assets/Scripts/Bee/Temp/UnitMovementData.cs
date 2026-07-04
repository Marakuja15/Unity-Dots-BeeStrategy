using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;


public struct UnitMovementData : IComponentData, IEnableableComponent
{

    public float3 moveLocation;

}