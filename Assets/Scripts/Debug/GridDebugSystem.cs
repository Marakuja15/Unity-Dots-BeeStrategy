using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class GridDebugSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GridData>();
        RequireForUpdate<GridSystemData>();
    }

    protected override void OnUpdate()
    {
        var gridData = SystemAPI.GetSingleton<GridData>();
        if (!gridData.Grid.IsCreated) return;

        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
        // Zbierzmy klucze z mapy kwiatów (żeby narysować nieodkryte na czerwono)
        var flowerKeys = gridData.Grid.GetKeyArray(Allocator.Temp);
        // Zbierzmy klucze z mapy odkrytych kratek (na zielono)
        var discoveredKeys = gridData.DiscoveredCells.GetKeyArray(Allocator.Temp);
        
        var uniqueCells = new NativeHashSet<int2>(flowerKeys.Length + discoveredKeys.Length, Allocator.Temp);

        // Funkcja pomocnicza do rysowania
        void DrawCell(int2 cell, Color color)
        {
            float3 bottomLeft = new float3(cell.x * cellSize, 0, cell.y * cellSize);
            float3 bottomRight = new float3((cell.x + 1) * cellSize, 0, cell.y * cellSize);
            float3 topLeft = new float3(cell.x * cellSize, 0, (cell.y + 1) * cellSize);
            float3 topRight = new float3((cell.x + 1) * cellSize, 0, (cell.y + 1) * cellSize);

            Debug.DrawLine(bottomLeft, bottomRight, color);
            Debug.DrawLine(bottomRight, topRight, color);
            Debug.DrawLine(topRight, topLeft, color);
            Debug.DrawLine(topLeft, bottomLeft, color);
            
            // Przekątne
            Debug.DrawLine(bottomLeft, topRight, new Color(color.r, color.g, color.b, 0.2f));
            Debug.DrawLine(topLeft, bottomRight, new Color(color.r, color.g, color.b, 0.2f));
        }

        // 1. Rysujemy na ZIELONO wszystkie kratki, które pszczoły już odkryły
        foreach (var cell in discoveredKeys)
        {
            if (uniqueCells.Add(cell))
            {
                DrawCell(cell, Color.green);
            }
        }

        // 2. Rysujemy na CZERWONO kratki, w których rosną kwiaty, ale pszczoły jeszcze ich nie odkryły
        foreach (var cell in flowerKeys)
        {
            if (uniqueCells.Add(cell)) // Jeśli się dodało, to znaczy, że nie było w odkrytych (bo inaczej dodalibyśmy to wyżej)
            {
                DrawCell(cell, Color.red);
            }
        }
        
        uniqueCells.Dispose();
        flowerKeys.Dispose();
        discoveredKeys.Dispose();
    }
}
