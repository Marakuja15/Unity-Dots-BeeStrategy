using Unity.Entities;
using Unity.Burst;

[UpdateBefore(typeof(ReturnToHiveSystem))]
public partial struct UnloadPollenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (carrier, hiveAssignment, team, flowerAssgnEnabled, returnEnabled, beeEntity) in
                 SystemAPI.Query<RefRW<BeeCarrier>, RefRO<HiveAssignment>, RefRO<TeamData>,
                 EnabledRefRW<NeedsFlowerAssignment>,
                 EnabledRefRW<ReturnToHive>>()
                 .WithDisabled<BeeMovementData, NeedsFlowerAssignment>()
                 .WithAll<ReturnToHive, PollenCollector>().WithEntityAccess())
        {
            if (hiveAssignment.ValueRO.currentHive == Entity.Null) continue;

            var hiveResources = SystemAPI.GetComponentRW<HiveResources>(hiveAssignment.ValueRO.currentHive);
            var hiveBuffer = SystemAPI.GetBuffer<PollenStorage>(hiveAssignment.ValueRO.currentHive);
            var beeBuffer = SystemAPI.GetBuffer<PollenStorage>(beeEntity);
            for (int i = 0; i < beeBuffer.Length; i++)
            {
                var hiveSlot = hiveBuffer[i];
                hiveSlot.Amount += beeBuffer[i].Amount;
                hiveBuffer[i] = hiveSlot;

                var beeSlot = beeBuffer[i];
                beeSlot.Amount = 0;
                beeBuffer[i] = beeSlot;
            }
            hiveResources.ValueRW.storedNectar += carrier.ValueRO.collectedNectar;
            carrier.ValueRW.collectedNectar = 0;

            returnEnabled.ValueRW = false;
            flowerAssgnEnabled.ValueRW = true;
        }
    }
}