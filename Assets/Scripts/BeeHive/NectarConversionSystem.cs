using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

public partial struct NectarConversionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (resources, population) in
                 SystemAPI.Query<RefRW<HiveResources>,
                  RefRO<HivePopulation>>())
        {
            float conversionRate = 5f;
            for (int i = 0; i < population.ValueRO.conversionWorkers; i++)
            {
                conversionRate += 0.5f;
            }
            float deltaTime = SystemAPI.Time.DeltaTime;
            float nectarToConvert = math.min(conversionRate * deltaTime, resources.ValueRO.storedNectar);

            resources.ValueRW.storedWax   += nectarToConvert * resources.ValueRO.waxRatio;
            resources.ValueRW.storedHoney += nectarToConvert * (1f - resources.ValueRO.waxRatio);
            resources.ValueRW.storedNectar -= nectarToConvert;
        }
    }
}