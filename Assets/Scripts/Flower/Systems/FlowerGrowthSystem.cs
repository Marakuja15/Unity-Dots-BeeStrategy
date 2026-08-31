using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct FlowerGrowthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
     
        float currentDeltaTime = SystemAPI.Time.DeltaTime;
        float ElapsedTime = (float)SystemAPI.Time.ElapsedTime;
        
        new FlowerGrowthJob
        { 
            deltaTime = currentDeltaTime,
            elapsedTime = ElapsedTime
            
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct FlowerGrowthJob : IJobEntity
{
    public float deltaTime;
    public float elapsedTime;

  void Execute(
    ref GrowthData growData,
    EnabledRefRW<GrowthData> grownEnabled,
    EnabledRefRW<FlowerData> flowerEnabled
  )
   
    {
        if(growData.growth >= growData.required)
        {
            flowerEnabled.ValueRW = true;
            grownEnabled.ValueRW = false;
            return;
        }
        growData.growth += growData.increment;
       
    }
}