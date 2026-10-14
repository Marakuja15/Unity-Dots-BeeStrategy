using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(PollenCollectorSystem))]
[UpdateAfter(typeof(UnloadPollenSystem))]
public partial struct ReturnToHiveSystem : ISystem
{
    private const float BeeHoverHeight = 0.75f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ReturnToHive>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (hiveAssignment, movement, task, transform, team, movementEnabled) in
                 SystemAPI.Query<RefRW<HiveAssignment>, RefRW<BeeMovementData>, RefRW<BeeTaskState>,
                     RefRO<LocalTransform>, RefRO<TeamData>,
                 EnabledRefRW<BeeMovementData>>()
                 .WithDisabled<BeeMovementData>()
                 .WithAll<ReturnToHive>())
        {
            float3 beePos = transform.ValueRO.Position;
            float closestDist = float.MaxValue;
            Entity closestHive = Entity.Null;

            foreach (var (hiveTransform, hiveTeam, hiveEntity) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<TeamData>>()
                     .WithAll<HiveResources>()
                     .WithEntityAccess())
            {
                if (hiveTeam.ValueRO.TeamID != team.ValueRO.TeamID) continue;

                float3 hiveApproachPosition = hiveTransform.ValueRO.Position + new float3(0f, BeeHoverHeight, 0f);
                float dist = math.distance(beePos, hiveApproachPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestHive = hiveEntity;
                    movement.ValueRW.moveLocation = hiveApproachPosition;
                }
            }

            if (closestHive != Entity.Null)
            {
                movementEnabled.ValueRW = true;
                hiveAssignment.ValueRW.currentHive = closestHive;
                task.ValueRW.Activity = BeeActivity.ReturningToHive;
                task.ValueRW.TargetEntity = closestHive;
                task.ValueRW.TargetPosition = movement.ValueRO.moveLocation;
            }
            else
            {
                task.ValueRW.Activity = BeeActivity.WaitingForHive;
                task.ValueRW.TargetEntity = Entity.Null;
                task.ValueRW.TargetPosition = float3.zero;
            }
        }
    }
}
