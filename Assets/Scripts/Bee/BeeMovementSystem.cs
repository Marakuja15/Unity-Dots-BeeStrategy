using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public partial struct BeeMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float currentDeltaTime = SystemAPI.Time.DeltaTime;
        float ElapsedTime = (float)SystemAPI.Time.ElapsedTime;
        
      
        var gridData = SystemAPI.GetSingleton<GridData>();
        int cellSize = SystemAPI.GetSingleton<GridData>().cellSize;
        
        new BeeMovementJob 
        { 
            deltaTime = currentDeltaTime,
            elapsedTime = ElapsedTime,
            cellSize = cellSize,
            discoveredWriter = gridData.DiscoveredCells.AsParallelWriter()
        }.ScheduleParallel();
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
        EnabledRefRW<BeeMovementData> movementEnabled)
    {
        float3 start = transform.Position;
        float3 destination = movementData.moveLocation; 
        float3 direction = destination - start;
    
        float distance = math.length(direction);
        
        if(distance <  movementData.stopRadius) 
        { 
            movementEnabled.ValueRW = false; 
            return; 
        }
        
        float3 normalizeDir = math.normalize(direction);
        float3 rightVector = math.cross(normalizeDir, math.up());
        
        float wobble = math.sin(elapsedTime * movementData.wobbleFrequency + movementData.randomOffSet)
         * movementData.wobbleAmplitude;
        float3 finalVelocity = (normalizeDir * movementData.speed) + (rightVector * wobble);
        
        transform.Position += finalVelocity * deltaTime;
        transform.Rotation = quaternion.LookRotationSafe(finalVelocity, math.up());

  
        int2 currentCell = new int2(
            (int)math.floor(transform.Position.x / cellSize),
            (int)math.floor(transform.Position.z / cellSize)
        );
  
        discoveredWriter.TryAdd(currentCell, true);
    }
}