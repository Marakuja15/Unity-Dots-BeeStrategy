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

public enum BeeActivity : byte
{
    SearchingForFlower,
    Scouting,
    FlyingToFlower,
    Collecting,
    ReturningToHive,
    WaitingForFlowers,
    WaitingForHive
}

public struct BeeTaskState : IComponentData
{
    public BeeActivity Activity;
    public Entity TargetEntity;
    public float3 TargetPosition;
}
