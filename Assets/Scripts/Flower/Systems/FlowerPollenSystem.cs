using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public partial struct FlowerPollenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (flower, flowerEnabled, growthEnabled) in
            SystemAPI.Query<RefRW<FlowerData>,
            EnabledRefRW<FlowerData>,
            EnabledRefRW<GrowthData>>()
            .WithDisabled<GrowthData>())
        {
            if (flower.ValueRO.owner == Entity.Null) continue;
            if (SystemAPI.IsComponentEnabled<BeeMovementData>(flower.ValueRO.owner)) continue;
            if (!SystemAPI.TryGetComponent<BeeCarrier>(flower.ValueRO.owner, out var carrier)) continue;

            float collectSpeed = carrier.collectSpeed;

            // Pollen collection
            float pollenToCollect = math.min(collectSpeed * deltaTime, flower.ValueRO.pollen);
            flower.ValueRW.pollen -= pollenToCollect;

            var beeBuffer = SystemAPI.GetBuffer<PollenStorage>(flower.ValueRO.owner);
            int idx = (int)flower.ValueRO.type;
            var slot = beeBuffer[idx];
            slot.Amount += pollenToCollect;
            beeBuffer[idx] = slot;

            // Nectar collection
            float nectarToCollect = math.min(collectSpeed * deltaTime, flower.ValueRO.nectar);
            flower.ValueRW.nectar -= nectarToCollect;

            var bee = SystemAPI.GetComponentRW<BeeCarrier>(flower.ValueRO.owner);
            bee.ValueRW.collectedNectar += nectarToCollect;

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