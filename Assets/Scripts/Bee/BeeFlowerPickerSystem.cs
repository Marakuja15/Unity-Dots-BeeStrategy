using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct BeeFlowerPickerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
     
        float currentDeltaTime = SystemAPI.Time.DeltaTime;
        float ElapsedTime = (float)SystemAPI.Time.ElapsedTime;
        
        new BeeFlowerPickerJob 
        { 
            deltaTime = currentDeltaTime,
            elapsedTime = ElapsedTime
            
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct BeeFlowerPickerJob : IJobEntity
{
    public float deltaTime;
    public float elapsedTime;
     public ComponentLookup<FlowerData> flowerLookup;
    void Execute(
        Entity beeEntity,
        ref BeeData beeData)
    {
         Entity targetFlower = beeData.targetFlower; 

         if (targetFlower == Entity.Null) return;
         FlowerData flower = flowerLookup[targetFlower];
        if(flower.owner == Entity.Null)
        {
            flower.owner = beeEntity;  
            flowerLookup[targetFlower] = flower; 
        }
       
    }
}