using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public partial struct BeeHiveGridSystem : ISystem
{

    public void OnUpdate(ref SystemState state)
    {
        ref var gridData = ref SystemAPI.GetSingletonRW<GridData>().ValueRW;
        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
        gridData.TeamHives.Clear();
         foreach (var (beeHive, team, entity) in 
            SystemAPI.Query<RefRO<BeeHiveData>,
            RefRO<TeamData>>().WithEntityAccess())
        {
            gridData.TeamHives.Add(team.ValueRO.TeamID, entity);
        }
    }
}
