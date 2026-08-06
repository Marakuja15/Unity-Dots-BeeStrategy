
using System;
using Unity.Entities;



public struct BeeData : IComponentData
{
  
    public float health;

    public float maxPollen;

    public Entity currentHive;



}