using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

public partial struct ReturnToHiveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        foreach (var (beeData, movement, transform, team, movementEnabled) in
                 SystemAPI.Query<RefRW<BeeData>, RefRW<BeeMovementData>, RefRO<LocalTransform>, RefRO<TeamData>,
                 EnabledRefRW<BeeMovementData>>()
                 .WithDisabled<BeeMovementData>()
                 .WithAll<ReturnToHive>())
        {
      
            float3 beePos = transform.ValueRO.Position;
            float closestDist = float.MaxValue;
            Entity closestHive = Entity.Null;

      
            foreach (var (hiveData, hiveTransform, hiveTeam, hiveEntity) in
                     SystemAPI.Query<RefRO<BeeHiveData>, RefRO<LocalTransform>, RefRO<TeamData>>()
                     .WithEntityAccess())
            {
                if (hiveTeam.ValueRO.TeamID != team.ValueRO.TeamID) continue;

                float dist = math.distance(beePos, hiveTransform.ValueRO.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestHive = hiveEntity;
                    movement.ValueRW.moveLocation = hiveTransform.ValueRO.Position;
                }
            }
            

            if (closestHive != Entity.Null)
            {
                movementEnabled.ValueRW = true;
                beeData.ValueRW.currentHive = closestHive;
            }
        }
    }
}