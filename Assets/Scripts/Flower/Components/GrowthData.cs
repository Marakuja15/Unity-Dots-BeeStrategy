
using System;
using Unity.Entities;
using Unity.Mathematics;


public struct GrowthData : IComponentData, IEnableableComponent
{
  
    public int growth;
    public int required;
    public int increment;
}