using System;

public static class FlowerTypeInfo
{
    public static int GetGrowthIncrement(FlowerType type) => type switch
    {
        FlowerType.Dandelion => 3,
        FlowerType.Tulip     => 5,
        FlowerType.Sunflower => 8,
        _ => 5
    };
    public static int GetGrowthRequired(FlowerType type) => type switch
    {
           FlowerType.Dandelion => 50,
        FlowerType.Tulip     => 100,
        FlowerType.Sunflower => 200,
        _ => 50
    };

    public static float GetPollen(FlowerType type) => type switch
    {
        FlowerType.Dandelion => 10f,
        FlowerType.Tulip     => 20f,
        FlowerType.Sunflower => 50f,
        _ => 10f
    };
    public static FlowerType[] GetAllTypes()
    {
        return (FlowerType[])Enum.GetValues(typeof(FlowerType));
    }

    public static float GetNectar(FlowerType type) => type switch
    {
        FlowerType.Dandelion => 10f,
        FlowerType.Tulip => 5f,
        FlowerType.Sunflower => 1f,
        _ => 1f
        
    };
}