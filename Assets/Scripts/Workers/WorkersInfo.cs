public static class WorkerCostInfo
{
  
    public static float GetSalary(WorkerType type) => type switch
    {
        WorkerType.ConversionWorker  => 0.5f,
        WorkerType.ConstructionWorker => 1.0f,
        WorkerType.Defender           => 2.0f,
        _ => 0.5f
    };
}

public enum WorkerType
{
    ConversionWorker,
    ConstructionWorker,
    Defender,
    Attacker,
    PollenCollector,
    Scout,
    
}