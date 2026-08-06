using Unity.Entities;
using Unity.Burst;


[UpdateBefore(typeof(ReturnToHiveSystem))]
public partial struct UnloadPollenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (beeData, team, flowerAssgnEnabled,returnEnabled, beeEntity) in
                 SystemAPI.Query<RefRW<BeeData>, RefRO<TeamData>,
                 EnabledRefRW<NeedsFlowerAssignment>,
                 EnabledRefRW<ReturnToHive>>()
                 .WithDisabled<BeeMovementData, NeedsFlowerAssignment>()
                 .WithAll<ReturnToHive, PollenCollector>().WithEntityAccess()
                
                 )
        {
                if(beeData.ValueRO.currentHive == Entity.Null) continue;
                
                var hiveData = SystemAPI.GetComponentRW<BeeHiveData>(beeData.ValueRO.currentHive);
                var hiveBuffer = SystemAPI.GetBuffer<PollenStorage>(beeData.ValueRO.currentHive);
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

            

                returnEnabled.ValueRW = false;
                flowerAssgnEnabled.ValueRW = true;


        }
    }
}