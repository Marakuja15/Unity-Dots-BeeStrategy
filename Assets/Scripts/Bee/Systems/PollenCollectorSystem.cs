using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
[UpdateBefore(typeof(ScoutSystem))]
public partial struct PollenCollectorSystem : ISystem
{
    private EntityQuery pollenCollectorQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PollenCollector>();
        state.RequireForUpdate<GridData>();
        state.RequireForUpdate<GridSystemData>();

        pollenCollectorQuery = SystemAPI.QueryBuilder()
            .WithAll<PollenCollector, NeedsFlowerAssignment, BeeCarrier, BeeTaskState,
                LocalTransform, PollenStorage>()
            .WithDisabled<BeeMovementData>()
            .WithDisabled<ReturnToHive>()
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (pollenCollectorQuery.IsEmpty) return;

        var gridData = SystemAPI.GetSingleton<GridData>();
        if (gridData.Grid.Count() == 0) return;

        int cellSize = math.max(1, SystemAPI.GetSingleton<GridSystemData>().cellSize);
        int capacity = math.max(64, pollenCollectorQuery.CalculateEntityCount());
        var takenFlowers = new NativeHashSet<Entity>(capacity, Allocator.TempJob);

        var job = new PollenCollectorJob
        {
            grid = gridData.Grid,
            cellSize = cellSize,
            takenFlowers = takenFlowers,
            flowerLookup = SystemAPI.GetComponentLookup<FlowerData>(false),
            transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true)
        };

        state.Dependency = job.Schedule(pollenCollectorQuery, state.Dependency);
        state.Dependency.Complete();
        takenFlowers.Dispose();
    }
}

[BurstCompile]
public partial struct PollenCollectorJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> grid;
    public int cellSize;
    public NativeHashSet<Entity> takenFlowers;
    public ComponentLookup<FlowerData> flowerLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;

    void Execute(
        Entity beeEntity,
        ref BeeMovementData movement,
        ref BeeCarrier carrier,
        ref BeeTaskState task,
        in LocalTransform transform,
        EnabledRefRW<NeedsFlowerAssignment> needsFlower,
        EnabledRefRW<ReturnToHive> returning,
        EnabledRefRW<BeeMovementData> movementEnabled,
        DynamicBuffer<PollenStorage> pollenBuffer)
    {
        if (!needsFlower.ValueRO || returning.ValueRO)
            return;

        float totalPollen = 0f;
        for (int i = 0; i < pollenBuffer.Length; i++)
            totalPollen += pollenBuffer[i].Amount;

        if (totalPollen >= math.max(0f, carrier.maxPollen))
        {
            needsFlower.ValueRW = false;
            returning.ValueRW = true;
            movementEnabled.ValueRW = false;
            task.Activity = BeeActivity.ReturningToHive;
            task.TargetEntity = Entity.Null;
            task.TargetPosition = float3.zero;
            return;
        }

        float3 beePosition = transform.Position;
        int2 beeCell = new int2(
            (int)math.floor(beePosition.x / cellSize),
            (int)math.floor(beePosition.z / cellSize));

        float closestDistanceSq = float.MaxValue;
        Entity closestFlower = Entity.Null;
        float3 closestPosition = float3.zero;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                int2 cell = beeCell + new int2(x, z);
                if (!grid.TryGetFirstValue(cell, out Entity flowerEntity, out var iterator))
                    continue;

                do
                {
                    if (takenFlowers.Contains(flowerEntity)) continue;
                    if (!flowerLookup.HasComponent(flowerEntity) || !transformLookup.HasComponent(flowerEntity)) continue;

                    var flower = flowerLookup[flowerEntity];
                    if (flower.owner != Entity.Null || flower.pollen <= 0f) continue;

                    float3 flowerPosition = transformLookup[flowerEntity].Position;
                    float distanceSq = math.distancesq(beePosition, flowerPosition);
                    if (distanceSq < closestDistanceSq)
                    {
                        closestDistanceSq = distanceSq;
                        closestFlower = flowerEntity;
                        closestPosition = flowerPosition + new float3(0f, 0.75f, 0f);
                    }
                }
                while (grid.TryGetNextValue(out flowerEntity, ref iterator));
            }
        }

        if (closestFlower == Entity.Null || takenFlowers.Contains(closestFlower))
            return;
        takenFlowers.Add(closestFlower);

        var assignedFlower = flowerLookup[closestFlower];
        assignedFlower.owner = beeEntity;
        flowerLookup[closestFlower] = assignedFlower;

        movement.moveLocation = closestPosition;
        movementEnabled.ValueRW = true;
        needsFlower.ValueRW = false;
        task.Activity = BeeActivity.FlyingToFlower;
        task.TargetEntity = closestFlower;
        task.TargetPosition = closestPosition;
    }
}
