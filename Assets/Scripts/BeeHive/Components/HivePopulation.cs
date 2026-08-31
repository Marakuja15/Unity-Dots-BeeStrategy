using Unity.Entities;

public struct HivePopulation : IComponentData
{
    public int defenders;
    public int citizens;
    public int conversionWorkers;

    public int uneducatedBees;
    public int educatedBees;
}
