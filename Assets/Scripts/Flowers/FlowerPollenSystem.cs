using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct FlowerPollenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (flower, flowerEnabled, growthEnabled) in
            SystemAPI.Query<RefRW<FlowerData>,
            EnabledRefRW<FlowerData>,
            EnabledRefRW<GrowthData>>())
        {
            if (flower.ValueRO.owner == Entity.Null) continue;
            
            flower.ValueRW.pollen -= 1;
            var bee = SystemAPI.GetComponentRW<BeeData>(flower.ValueRO.owner);
            bee.ValueRW.collectedPollen += 1;
            
            if (flower.ValueRO.pollen <= 0)
            {
                flower.ValueRW.owner = Entity.Null;
                flowerEnabled.ValueRW = false;
                growthEnabled.ValueRW = true;
            }
        }
    }
}