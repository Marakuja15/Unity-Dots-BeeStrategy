using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct BeeHiveGridSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        ref var gridData = ref SystemAPI.GetSingletonRW<GridData>().ValueRW;
        gridData.TeamHives.Clear();
        foreach (var (team, entity) in
            SystemAPI.Query<RefRO<TeamData>>().WithAll<HiveResources>().WithEntityAccess())
        {
            gridData.TeamHives.Add(team.ValueRO.TeamID, entity);
        }
    }
}
