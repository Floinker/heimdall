using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherBuildable : StaticObject {

    private bool freshPlaced;
    
    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (IsPlaced() && !freshPlaced) {
            freshPlaced = true;
            var controller = this.GetComponent<ArcherController>();
            ArcherController.targetPositions[controller] = Vector3.zero;
        }
    }
    
}