using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;


public struct BeeMovementData : IComponentData, IEnableableComponent
{

    public float3 moveLocation;
  
    public float wobbleFrequency;

     public float randomOffSet;

     public float wobbleAmplitude;
    public float speed;
    public float stopRadius;
   

}