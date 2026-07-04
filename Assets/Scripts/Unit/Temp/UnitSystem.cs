using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct UnitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
     
        float currentDeltaTime = SystemAPI.Time.DeltaTime;

        
        new UnitMoveJob 
        { 
            deltaTime = currentDeltaTime 
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMoveJob : IJobEntity
{
    public float deltaTime;

  void Execute(
        ref LocalTransform transform, 
        in UnitData unitData, 
        ref UnitMovementData movementData, 
        EnabledRefRW<UnitMovementData> movementEnabled)
    {
        
        float3 start = transform.Position;
        float3 destination = movementData.moveLocation; 
        float3 direction = destination - start;
    
        float distance = math.length(direction);
        
        if(distance < unitData.stoppingDistance) 
        { 
           
            movementEnabled.ValueRW = false; 
            
            return; 
        }
        
        float3 normalizeDir = math.normalize(direction);
        transform.Position += normalizeDir * unitData.speed * deltaTime;
    }
}