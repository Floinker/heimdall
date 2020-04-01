using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ArrowData : IComponentData {
    public Vector3 startPos;
    public Vector3 endPos;
    public float progress;

    //for impact calculation - height of the minions
    public float minY;
    public float maxY;

    public bool done;
}
