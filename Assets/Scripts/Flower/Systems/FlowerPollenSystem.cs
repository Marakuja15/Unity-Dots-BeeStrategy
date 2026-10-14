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
                 SystemAPI.Query<RefRW<FlowerData>, EnabledRefRW<FlowerData>, EnabledRefRW<GrowthData>>()
                     .WithDisabled<GrowthData>())
        {
            Entity beeEntity = flower.ValueRO.owner;
            if (beeEntity == Entity.Null || !state.EntityManager.Exists(beeEntity)) continue;
            if (!SystemAPI.HasComponent<BeeCarrier>(beeEntity) ||
                !SystemAPI.HasBuffer<PollenStorage>(beeEntity)) continue;
            if (SystemAPI.IsComponentEnabled<BeeMovementData>(beeEntity)) continue;

            var carrier = SystemAPI.GetComponentRW<BeeCarrier>(beeEntity);
            var beeBuffer = SystemAPI.GetBuffer<PollenStorage>(beeEntity);
            int pollenIndex = (int)flower.ValueRO.type;
            if ((uint)pollenIndex >= (uint)beeBuffer.Length) continue;

            float totalPollen = 0f;
            for (int i = 0; i < beeBuffer.Length; i++)
                totalPollen += beeBuffer[i].Amount;

            float maxPollen = math.max(0f, carrier.ValueRO.maxPollen);
            float remainingPollen = math.max(0f, maxPollen - totalPollen);
            if (remainingPollen <= 0f)
            {
                SendBeeHome(beeEntity, ref state);
                flower.ValueRW.owner = Entity.Null;
                continue;
            }

            float collectAmount = math.max(0f, carrier.ValueRO.collectSpeed * deltaTime);
            float pollenToCollect = math.min(collectAmount,
                math.min(flower.ValueRO.pollen, remainingPollen));
            flower.ValueRW.pollen = math.max(0f, flower.ValueRO.pollen - pollenToCollect);

            var pollenSlot = beeBuffer[pollenIndex];
            pollenSlot.Amount += pollenToCollect;
            beeBuffer[pollenIndex] = pollenSlot;

            float remainingNectar = math.max(0f, carrier.ValueRO.maxNectar - carrier.ValueRO.collectedNectar);
            float nectarToCollect = math.min(collectAmount,
                math.min(flower.ValueRO.nectar, remainingNectar));
            flower.ValueRW.nectar = math.max(0f, flower.ValueRO.nectar - nectarToCollect);
            carrier.ValueRW.collectedNectar += nectarToCollect;

            float updatedTotalPollen = totalPollen + pollenToCollect;
            bool pollenCapacityReached = updatedTotalPollen >= maxPollen;
            bool flowerDepleted = flower.ValueRO.pollen <= 0f;

            if (flowerDepleted)
            {
                flowerEnabled.ValueRW = false;
                growthEnabled.ValueRW = true;
                flower.ValueRW.owner = Entity.Null;
            }

            if (pollenCapacityReached)
            {
                SendBeeHome(beeEntity, ref state);
                flower.ValueRW.owner = Entity.Null;
            }
            else if (flowerDepleted)
            {
                if (state.EntityManager.HasComponent<NeedsFlowerAssignment>(beeEntity))
                    state.EntityManager.SetComponentEnabled<NeedsFlowerAssignment>(beeEntity, true);
                if (state.EntityManager.HasComponent<ReturnToHive>(beeEntity))
                    state.EntityManager.SetComponentEnabled<ReturnToHive>(beeEntity, false);
                SetBeeTask(beeEntity, BeeActivity.SearchingForFlower, Entity.Null, float3.zero, ref state);
            }
        }
    }

    private static void SendBeeHome(Entity beeEntity, ref SystemState state)
    {
        if (state.EntityManager.HasComponent<NeedsFlowerAssignment>(beeEntity))
            state.EntityManager.SetComponentEnabled<NeedsFlowerAssignment>(beeEntity, false);
        if (state.EntityManager.HasComponent<ReturnToHive>(beeEntity))
            state.EntityManager.SetComponentEnabled<ReturnToHive>(beeEntity, true);
        if (state.EntityManager.HasComponent<BeeMovementData>(beeEntity))
            state.EntityManager.SetComponentEnabled<BeeMovementData>(beeEntity, false);
        SetBeeTask(beeEntity, BeeActivity.ReturningToHive, Entity.Null, float3.zero, ref state);
    }

    private static void SetBeeTask(
        Entity beeEntity,
        BeeActivity activity,
        Entity target,
        float3 targetPosition,
        ref SystemState state)
    {
        if (!state.EntityManager.HasComponent<BeeTaskState>(beeEntity)) return;
        var task = state.EntityManager.GetComponentData<BeeTaskState>(beeEntity);
        task.Activity = activity;
        task.TargetEntity = target;
        task.TargetPosition = targetPosition;
        state.EntityManager.SetComponentData(beeEntity, task);
    }
}
