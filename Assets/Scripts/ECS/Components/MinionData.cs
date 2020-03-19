using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MinionData : IComponentData {
    public bool isRigidbody;
}
