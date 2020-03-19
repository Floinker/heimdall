using System.Collections;
using System.Collections.Generic;
using NavJob.Systems;
using UnityEngine;

public class WallStone : StaticObject {
    public override void setIsPlaced(bool val) {
        base.setIsPlaced(val);
        if (val) NavMeshQuerySystem.updateWorld();
    }
}