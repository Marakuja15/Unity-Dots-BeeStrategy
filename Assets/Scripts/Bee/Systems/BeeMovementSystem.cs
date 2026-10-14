using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public partial struct BeeMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridData>();
        state.RequireForUpdate<GridSystemData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        float elapsedTime = (float)SystemAPI.Time.ElapsedTime;
        var gridData = SystemAPI.GetSingleton<GridData>();
        int cellSize = math.max(1, SystemAPI.GetSingleton<GridSystemData>().cellSize);

        state.Dependency = new BeeMovementJob
        {
            deltaTime = deltaTime,
            elapsedTime = elapsedTime,
            cellSize = cellSize,
            discoveredWriter = gridData.DiscoveredCells.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct BeeMovementJob : IJobEntity
{
    public float deltaTime;
    public float elapsedTime;
    public int cellSize;
    public NativeParallelHashMap<int2, bool>.ParallelWriter discoveredWriter;

    void Execute(
        ref LocalTransform transform, 
        ref BeeMovementData movementData, 
        ref BeeTaskState task,
        EnabledRefRW<BeeMovementData> movementEnabled)
    {
        float3 start = transform.Position;
        float3 destination = movementData.moveLocation; 
        float3 direction = destination - start;
        float distance = math.length(direction);

        if (distance <= math.max(0.05f, movementData.stopRadius))
        {
            movementEnabled.ValueRW = false; 

            if (task.Activity == BeeActivity.Scouting)
            {
                int2 currentCell = new int2(
                    (int)math.floor(start.x / cellSize),
                    (int)math.floor(start.z / cellSize));
                discoveredWriter.TryAdd(currentCell, true);
                task.Activity = BeeActivity.SearchingForFlower;
                task.TargetEntity = Entity.Null;
                task.TargetPosition = float3.zero;
            }
            else if (task.Activity == BeeActivity.FlyingToFlower)
            {
                task.Activity = BeeActivity.Collecting;
            }
            return; 
        }

        float3 normalizedDirection = math.normalizesafe(direction);
        float3 rightVector = math.normalizesafe(math.cross(normalizedDirection, math.up()));
        float wobble = math.sin(elapsedTime * movementData.wobbleFrequency + movementData.randomOffSet)
            * movementData.wobbleAmplitude
            * math.saturate(distance / math.max(1f, movementData.speed));
        float3 velocity = normalizedDirection * math.max(0f, movementData.speed) + rightVector * wobble;
        float3 displacement = velocity * deltaTime;
        float displacementLength = math.length(displacement);
        if (displacementLength > distance)
            displacement *= distance / displacementLength;

        transform.Position += displacement;
        if (task.Activity == BeeActivity.Scouting)
        {
            int2 visitedCell = new int2(
                (int)math.floor(transform.Position.x / cellSize),
                (int)math.floor(transform.Position.z / cellSize));
            discoveredWriter.TryAdd(visitedCell, true);
        }
        transform.Rotation = quaternion.LookRotationSafe(velocity, math.up());
    }
}
