using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BeeHiveSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // SpawnerData przechowuje nasz prefab pszczoły
        state.RequireForUpdate<SpawnerData>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // Używamy ParallelWriter, bo system działa wielowątkowo
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        if (!SystemAPI.TryGetSingleton<SpawnerData>(out var spawnerData)) return;

        float deltaTime = SystemAPI.Time.DeltaTime;
        Entity beePrefab = spawnerData.prefab;

        var job = new SpawnBeesFromHiveJob
        {
            Ecb = ecb,
            DeltaTime = deltaTime,
            BeePrefab = beePrefab
        };

        job.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct SpawnBeesFromHiveJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public float DeltaTime;
    public Entity BeePrefab;

    void Execute([ChunkIndexInQuery] int sortKey, Entity hiveEntity, ref HiveProduction production, ref HiveResources resources, ref HivePopulation population, in LocalTransform transform, in TeamData team)
    {
        // Jeśli produkcja w UI jest wyłączona (0 pszczół na 5 minut), wychodzimy
        if (production.TargetBeesPer5Min <= 0) return;

        // Przeliczenie: 5 minut to 300 sekund.
        float cooldown = 300f / production.TargetBeesPer5Min;
        production.SpawnTimer += DeltaTime;

        // Jeśli minął czas i jest gotowa do wyklucia
        if (production.SpawnTimer >= cooldown)
        {
            // Ul-Stolica płaci tylko 25, pozostałe 50
            int cost = production.IsCapital ? 25 : 50;
            
            // Czy ul ma wystarczająco dużo nektaru?
            if (resources.storedNectar >= cost)
            {
                // Odebranie surowców
                resources.storedNectar -= cost;
                production.SpawnTimer -= cooldown; // Odjęcie cooldownu, żeby timer "złapał" opóźnienia zamiast resetu na 0
                population.citizens += 1;

                // 1. Zespawnowanie z prefaba
                Entity newBee = Ecb.Instantiate(sortKey, BeePrefab);
                
                // 2. Pozycja narodzin (odrobinę nad ulem lub z boku)
                float3 spawnPos = transform.Position + new float3(0, 1f, 0);
                Ecb.SetComponent(sortKey, newBee, LocalTransform.FromPosition(spawnPos));
                
                // 3. Przypisanie do drużyny i ula macierzystego
                Ecb.AddComponent(sortKey, newBee, new TeamData { TeamID = team.TeamID });
                Ecb.SetComponent(sortKey, newBee, new HiveAssignment { currentHive = hiveEntity });
                
                // 4. Przypisanie ról i uruchomienie ruchu
                Ecb.AddComponent(sortKey, newBee, new PollenCollector());
                Ecb.AddComponent(sortKey, newBee, new NeedsFlowerAssignment());
                Ecb.AddComponent(sortKey, newBee, new Scout());
                
                Ecb.SetComponentEnabled<BeeMovementData>(sortKey, newBee, true);
            }
        }
    }
}
