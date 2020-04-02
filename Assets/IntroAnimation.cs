using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimation : MonoBehaviour {
    public static bool showAnim = true;
    public Animator animator;

    // Start is called before the first frame update
    void Start() {
        if (showAnim) {
            animator.enabled = true;
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.anyKey && animator.enabled) {
            AnalyticsHelper.introSkipped(Time.timeSinceLevelLoad);
            animator.Play("intro_camera", -1, 0.95f);
        }
    }
}