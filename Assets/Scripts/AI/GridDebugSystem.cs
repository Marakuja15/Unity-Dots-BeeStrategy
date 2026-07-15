using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
public partial class GridDebugSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GridSystemData>();
    }

    protected override void OnUpdate()
    {
        var gridSystem = World.GetExistingSystemManaged<GridSystem>();
        if (gridSystem == null || !gridSystem.Grid.IsCreated) return;

        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
        var keys = gridSystem.Grid.GetKeyArray(Allocator.Temp);
        
        // Używamy NativeHashSet, żeby nie rysować tej samej kratki wiele razy
        // (bo MultiHashMap zwraca klucz dla każdego kwiatka w danej kratce)
        var uniqueCells = new NativeHashSet<int2>(keys.Length, Allocator.Temp);

        foreach (var cell in keys)
        {
            if (uniqueCells.Add(cell))
            {
                // Obliczanie rogów kratki (Y = 0, bo to grid płaski)
                float3 bottomLeft = new float3(cell.x * cellSize, 0, cell.y * cellSize);
                float3 bottomRight = new float3((cell.x + 1) * cellSize, 0, cell.y * cellSize);
                float3 topLeft = new float3(cell.x * cellSize, 0, (cell.y + 1) * cellSize);
                float3 topRight = new float3((cell.x + 1) * cellSize, 0, (cell.y + 1) * cellSize);

                Color color = Color.green;

                // Rysowanie 4 linii tworzących kwadrat
                Debug.DrawLine(bottomLeft, bottomRight, color);
                Debug.DrawLine(bottomRight, topRight, color);
                Debug.DrawLine(topRight, topLeft, color);
                Debug.DrawLine(topLeft, bottomLeft, color);
                
                // Można też narysować przekątne, żeby kratka była bardziej widoczna
                Debug.DrawLine(bottomLeft, topRight, new Color(0f, 1f, 0f, 0.2f));
                Debug.DrawLine(topLeft, bottomRight, new Color(0f, 1f, 0f, 0.2f));
            }
        }
        
        uniqueCells.Dispose();
        keys.Dispose();
    }
}
