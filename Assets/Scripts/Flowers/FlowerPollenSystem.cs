using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct FlowerPollenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        float collectSpeed = 5f; 
        foreach (var (flower, flowerEnabled, growthEnabled) in
            SystemAPI.Query<RefRW<FlowerData>,
            EnabledRefRW<FlowerData>,
            EnabledRefRW<GrowthData>>()
            .WithDisabled<GrowthData>())
        {
            if (flower.ValueRO.owner == Entity.Null) continue;
            if (SystemAPI.IsComponentEnabled<BeeMovementData>(flower.ValueRO.owner)) continue;
          

            float amountToCollect = collectSpeed * deltaTime;
            
           
            if (amountToCollect > flower.ValueRO.pollen)
            {
                amountToCollect = flower.ValueRO.pollen;
            }

            flower.ValueRW.pollen -= amountToCollect;

            var bee = SystemAPI.GetComponentRW<BeeData>(flower.ValueRO.owner);
            var beeBuffer = SystemAPI.GetBuffer<PollenStorage>(flower.ValueRO.owner);
            int idx = (int)flower.ValueRO.type;
            var slot = beeBuffer[idx];
            slot.Amount += amountToCollect;
            beeBuffer[idx] = slot;
            
            if (flower.ValueRO.pollen <= 0)
            {
           
                flowerEnabled.ValueRW = false;
                growthEnabled.ValueRW = true;
                SystemAPI.SetComponentEnabled<NeedsFlowerAssignment>(flower.ValueRO.owner, true);
                flower.ValueRW.owner = Entity.Null;
               
          
            }

        }
    }
}