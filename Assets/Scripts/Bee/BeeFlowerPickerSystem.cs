using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct BeeFlowerPickerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeData>();
    }
    [BurstCompile]
    
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (beeData, entity) in 
            SystemAPI.Query<RefRW<BeeData>>().WithEntityAccess())
        {
            Entity targetFlower = beeData.ValueRO.targetFlower;
            if (targetFlower == Entity.Null) continue;

            var flower = SystemAPI.GetComponentRW<FlowerData>(targetFlower);
            if (flower.ValueRO.owner == Entity.Null)
            {
                flower.ValueRW.owner = entity;
            }
        }
    }
}