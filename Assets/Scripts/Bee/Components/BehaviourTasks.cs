using Unity.Entities;

public struct MaxPollen : IComponentData { }
public struct NeedsFlowerAssignment : IComponentData, IEnableableComponent { }

public struct ReturnToHive : IComponentData, IEnableableComponent {}