using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemUtility : MonoBehaviour {
    public ParticleSystem FX_sparks;
    public ParticleSystem FX_sparks_suba;
    public ParticleSystem FX_sparks_subb;
    ParticleSystem.EmitParams FX_params_sparks;
    private static ParticleSystemUtility instance;

    public static ParticleSystemUtility getInstance() {
        return instance;
    }

    void Awake() {
        instance = this;
        FX_params_sparks = new ParticleSystem.EmitParams();
    }

    public void doEmit(Vector3 pos, int count = 1) {
        FX_params_sparks.position = pos;
        FX_sparks.Emit(FX_params_sparks, count);
        FX_sparks_suba.Emit(FX_params_sparks, count);
        FX_sparks_subb.Emit(FX_params_sparks, count);
    }
}