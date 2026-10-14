using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

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
        BeeMovementData beeMovement = SystemAPI.GetComponent<BeeMovementData>(beePrefab);

        var job = new SpawnBeesFromHiveJob
        {
            Ecb = ecb,
            DeltaTime = deltaTime,
            BeePrefab = beePrefab,
            BeeMovement = beeMovement
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct SpawnBeesFromHiveJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public float DeltaTime;
    public Entity BeePrefab;
    public BeeMovementData BeeMovement;

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

                float3 spawnPos = transform.Position + new float3(0, 1f, 0);
                Ecb.SetComponent(sortKey, newBee, LocalTransform.FromPosition(spawnPos));
                var movement = BeeMovement;
                movement.moveLocation = spawnPos;
                Ecb.SetComponent(sortKey, newBee, movement);
                Ecb.SetComponentEnabled<BeeMovementData>(sortKey, newBee, false);

                Ecb.AddComponent(sortKey, newBee, new TeamData { TeamID = team.TeamID });
                Ecb.SetComponent(sortKey, newBee, new HiveAssignment { currentHive = hiveEntity });
                Ecb.SetComponent(sortKey, newBee, new BeeTaskState
                {
                    Activity = BeeActivity.SearchingForFlower,
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero
                });
                Ecb.AddComponent(sortKey, newBee, new PollenCollector());
                Ecb.AddComponent(sortKey, newBee, new NeedsFlowerAssignment());
                Ecb.AddComponent(sortKey, newBee, new Scout());
                Ecb.AddComponent(sortKey, newBee, new ReturnToHive());
                Ecb.SetComponentEnabled<ReturnToHive>(sortKey, newBee, false);
            }
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct InitialHiveSystem : ISystem
{
    private EntityQuery hiveQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitialHiveSpawnData>();
        state.RequireForUpdate<PlayerData>();
        hiveQuery = SystemAPI.QueryBuilder()
            .WithAll<HiveResources, TeamData>()
            .Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        var spawnData = SystemAPI.GetSingleton<InitialHiveSpawnData>();
        var authoringEntity = SystemAPI.GetSingletonEntity<InitialHiveSpawnData>();
        var player = SystemAPI.GetSingleton<PlayerData>();
        bool hasPlayerHive = false;

        using var hives = hiveQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < hives.Length; i++)
        {
            if (state.EntityManager.GetComponentData<TeamData>(hives[i]).TeamID == player.TeamID)
            {
                hasPlayerHive = true;
                break;
            }
        }

        if (!hasPlayerHive && spawnData.Prefab != Entity.Null &&
            state.EntityManager.Exists(spawnData.Prefab))
        {
            Entity hive = state.EntityManager.Instantiate(spawnData.Prefab);
            if (state.EntityManager.HasComponent<LocalTransform>(hive))
            {
                var transform = state.EntityManager.GetComponentData<LocalTransform>(hive);
                transform.Position = spawnData.Position;
                state.EntityManager.SetComponentData(hive, transform);
            }
            else
            {
                state.EntityManager.AddComponentData(hive, LocalTransform.FromPosition(spawnData.Position));
            }

            if (state.EntityManager.HasComponent<TeamData>(hive))
                state.EntityManager.SetComponentData(hive, new TeamData { TeamID = player.TeamID });
            else
                state.EntityManager.AddComponentData(hive, new TeamData { TeamID = player.TeamID });
        }

        state.EntityManager.RemoveComponent<InitialHiveSpawnData>(authoringEntity);
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(InitialHiveSystem))]
public partial struct HiveAssignmentRepairSystem : ISystem
{
    private EntityQuery beeQuery;
    private EntityQuery hiveQuery;
    private double nextRepairTime;

    public void OnCreate(ref SystemState state)
    {
        beeQuery = state.GetEntityQuery(
            ComponentType.ReadWrite<HiveAssignment>(),
            ComponentType.ReadOnly<TeamData>(),
            ComponentType.ReadOnly<LocalTransform>());
        hiveQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<HiveResources>(),
            ComponentType.ReadOnly<TeamData>(),
            ComponentType.ReadOnly<LocalTransform>());
    }

    public void OnUpdate(ref SystemState state)
    {
        double now = SystemAPI.Time.ElapsedTime;
        if (now < nextRepairTime || beeQuery.IsEmpty || hiveQuery.IsEmpty) return;
        nextRepairTime = now + 0.5;

        using var bees = beeQuery.ToEntityArray(Allocator.Temp);
        using var hives = hiveQuery.ToEntityArray(Allocator.Temp);
        var entityManager = state.EntityManager;

        for (int i = 0; i < bees.Length; i++)
        {
            Entity bee = bees[i];
            var beeTeam = entityManager.GetComponentData<TeamData>(bee);
            var assignment = entityManager.GetComponentData<HiveAssignment>(bee);

            if (IsValidFriendlyHive(assignment.currentHive, beeTeam.TeamID, ref entityManager))
                continue;

            float3 beePosition = entityManager.GetComponentData<LocalTransform>(bee).Position;
            float closestDistanceSq = float.MaxValue;
            Entity closestHive = Entity.Null;

            for (int j = 0; j < hives.Length; j++)
            {
                Entity hive = hives[j];
                if (entityManager.GetComponentData<TeamData>(hive).TeamID != beeTeam.TeamID)
                    continue;

                float distanceSq = math.distancesq(
                    beePosition, entityManager.GetComponentData<LocalTransform>(hive).Position);
                if (distanceSq >= closestDistanceSq) continue;
                closestDistanceSq = distanceSq;
                closestHive = hive;
            }

            if (closestHive != Entity.Null)
            {
                bool wasUnassigned = assignment.currentHive == Entity.Null;
                assignment.currentHive = closestHive;
                entityManager.SetComponentData(bee, assignment);

                if (wasUnassigned && entityManager.HasComponent<HivePopulation>(closestHive))
                {
                    var population = entityManager.GetComponentData<HivePopulation>(closestHive);
                    population.citizens++;
                    entityManager.SetComponentData(closestHive, population);
                }
            }
        }
    }

    private static bool IsValidFriendlyHive(Entity hive, byte beeTeam, ref EntityManager entityManager)
    {
        return hive != Entity.Null && entityManager.Exists(hive) &&
               entityManager.HasComponent<HiveResources>(hive) &&
               entityManager.HasComponent<TeamData>(hive) &&
               entityManager.HasComponent<LocalTransform>(hive) &&
               entityManager.GetComponentData<TeamData>(hive).TeamID == beeTeam;
    }
}
