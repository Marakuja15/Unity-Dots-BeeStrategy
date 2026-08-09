using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;


public partial struct NectarConversionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (hiveData, team) in
                 SystemAPI.Query<RefRW<BeeHiveData>,
                  RefRO<TeamData>>()
                
                 )
        {
          
            
            float conversionRate = 5f;
            for(int i = 0; i < hiveData.ValueRO.conversionWorkers; i++)
            {
                conversionRate += 0.5f;
            }
            float deltaTime = SystemAPI.Time.DeltaTime;
            float nectarToConvert = math.min(conversionRate * deltaTime, hiveData.ValueRO.storedNectar);

            hiveData.ValueRW.storedWax   += nectarToConvert * hiveData.ValueRO.waxRatio;
            hiveData.ValueRW.storedHoney += nectarToConvert * (1f - hiveData.ValueRO.waxRatio);
            hiveData.ValueRW.storedNectar -= nectarToConvert;
            


        }
    }
}