using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct FlowerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
     
        float currentDeltaTime = SystemAPI.Time.DeltaTime;
        float ElapsedTime = (float)SystemAPI.Time.ElapsedTime;
        
        new FlowerJob
        { 
            deltaTime = currentDeltaTime,
            elapsedTime = ElapsedTime
            
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct FlowerJob : IJobEntity
{
    public float deltaTime;
    public float elapsedTime;

  void Execute(
    ref FlowerData flower,
    EnabledRefRW<FlowerData> flowerEnabled,
    EnabledRefRW<GrowthData> growthEnabled
  )
   
    {
        if(flower.pollen <= 0)
        {
            flowerEnabled.ValueRW = false;
            growthEnabled.ValueRW = true;
            
        }
        if(flower.owner != Entity.Null)
        {
            flower.pollen -= 1;
        }
       
    }
}