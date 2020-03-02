using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MinionData : IComponentData {
    public Int32 trackedID;
    public float3 initialOffset;
}
