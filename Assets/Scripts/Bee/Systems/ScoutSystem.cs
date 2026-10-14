using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(PollenCollectorSystem))]
public partial struct ScoutSystem : ISystem
{
    private EntityQuery scoutQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Scout>();
        state.RequireForUpdate<GridData>();
        state.RequireForUpdate<GridSystemData>();
        state.RequireForUpdate<FlowerSpawnerData>();

        scoutQuery = SystemAPI.QueryBuilder()
            .WithAll<Scout, NeedsFlowerAssignment, BeeTaskState, LocalTransform>()
            .WithDisabled<BeeMovementData>()
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (scoutQuery.IsEmpty) return;

        var gridData = SystemAPI.GetSingleton<GridData>();
        var spawner = SystemAPI.GetSingleton<FlowerSpawnerData>();
        if (gridData.Grid.Count() == 0)
        {
            foreach (var task in SystemAPI.Query<RefRW<BeeTaskState>>()
                         .WithAll<Scout, NeedsFlowerAssignment>()
                         .WithDisabled<BeeMovementData>())
            {
                task.ValueRW.Activity = BeeActivity.WaitingForFlowers;
                task.ValueRW.TargetEntity = Entity.Null;
                task.ValueRW.TargetPosition = float3.zero;
            }
            return;
        }

        int cellSize = math.max(1, SystemAPI.GetSingleton<GridSystemData>().cellSize);
        int2 minCell = new int2(
            (int)math.floor(math.min(spawner.spawnLeft.x, spawner.spawnRight.x) / cellSize),
            (int)math.floor(math.min(spawner.spawnLeft.z, spawner.spawnRight.z) / cellSize));
        int2 maxCell = new int2(
            (int)math.floor(math.max(spawner.spawnLeft.x, spawner.spawnRight.x) / cellSize),
            (int)math.floor(math.max(spawner.spawnLeft.z, spawner.spawnRight.z) / cellSize));

        var claimedCells = new NativeParallelHashSet<int2>(
            math.max(1, scoutQuery.CalculateEntityCount()), Allocator.TempJob);

        var job = new FindScoutTargetJob
        {
            cellSize = cellSize,
            minCell = minCell,
            maxCell = maxCell,
            randomSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1,
            discoveredCells = gridData.DiscoveredCells,
            claimedCells = claimedCells.AsParallelWriter()
        };

        state.Dependency = job.ScheduleParallel(scoutQuery, state.Dependency);
        state.Dependency.Complete();
        claimedCells.Dispose();
    }
}

[BurstCompile]
public partial struct FindScoutTargetJob : IJobEntity
{
    public int cellSize;
    public int2 minCell;
    public int2 maxCell;
    public uint randomSeed;

    [ReadOnly]
    public NativeParallelHashMap<int2, bool> discoveredCells;
    public NativeParallelHashSet<int2>.ParallelWriter claimedCells;

    void Execute(
        Entity beeEntity,
        ref BeeMovementData movement,
        ref BeeTaskState task,
        in LocalTransform transform,
        EnabledRefRW<BeeMovementData> movementEnabled)
    {
        int width = math.max(1, maxCell.x - minCell.x + 1);
        int height = math.max(1, maxCell.y - minCell.y + 1);
        int cellCount = width * height;
        uint seed = randomSeed + (uint)beeEntity.Index + 1u;
        if (seed == 0) seed = 1;
        var random = Random.CreateFromIndex(seed);

        int2 targetCell = minCell;
        bool foundTarget = false;
        int randomAttempts = math.min(cellCount, 64);

        for (int i = 0; i < randomAttempts; i++)
        {
            var candidate = new int2(
                random.NextInt(minCell.x, maxCell.x + 1),
                random.NextInt(minCell.y, maxCell.y + 1));
            if (discoveredCells.ContainsKey(candidate) || !claimedCells.TryAdd(candidate)) continue;

            targetCell = candidate;
            foundTarget = true;
            break;
        }

        if (!foundTarget && cellCount <= 4096)
        {
            for (int x = minCell.x; x <= maxCell.x && !foundTarget; x++)
            {
                for (int z = minCell.y; z <= maxCell.y; z++)
                {
                    var candidate = new int2(x, z);
                    if (discoveredCells.ContainsKey(candidate) || !claimedCells.TryAdd(candidate)) continue;
                    targetCell = candidate;
                    foundTarget = true;
                    break;
                }
            }
        }

        if (!foundTarget && cellCount > 1)
        {
            int2 currentCell = new int2(
                (int)math.floor(transform.Position.x / cellSize),
                (int)math.floor(transform.Position.z / cellSize));
            for (int i = 0; i < 16; i++)
            {
                var candidate = new int2(
                    random.NextInt(minCell.x, maxCell.x + 1),
                    random.NextInt(minCell.y, maxCell.y + 1));
                if (candidate.Equals(currentCell) || !claimedCells.TryAdd(candidate)) continue;
                targetCell = candidate;
                foundTarget = true;
                break;
            }
        }

        if (!foundTarget)
        {
            movementEnabled.ValueRW = false;
            task.Activity = BeeActivity.WaitingForFlowers;
            task.TargetEntity = Entity.Null;
            task.TargetPosition = transform.Position;
            return;
        }

        float3 targetPosition = new float3(
            targetCell.x * cellSize + cellSize * 0.5f,
            transform.Position.y,
            targetCell.y * cellSize + cellSize * 0.5f);

        movement.moveLocation = targetPosition;
        movementEnabled.ValueRW = true;
        task.Activity = BeeActivity.Scouting;
        task.TargetEntity = Entity.Null;
        task.TargetPosition = targetPosition;
    }
}
