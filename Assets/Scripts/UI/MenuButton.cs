using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorFunctions animatorFunctions;
    [SerializeField] int thisIndex;

    // Update is called once per frame
    void Update() {
        if (menuButtonController.index == thisIndex) {
            animator.SetBool("selected", true);
            if (Input.GetAxis("Submit") == 1) {
                animator.SetTrigger("pressedTrigger");
            }
            else if (animator.GetBool("pressed")) {
                animator.SetBool("pressed", false);
                animatorFunctions.disableOnce = true;

                //Check if animation is finished
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0)) {
                    switch (thisIndex) {
                        //Play Button
                        case 0:
                            break;
                        //Options Button
                        case 1:
                            break;
                        //Quit Button
                        case 2:
                            Application.Quit();
                            break;
                    }
                }
            }
        }
        else {
            animator.SetBool("selected", false);
        }
    }

    public void startPlayAnimManual() {
        print("starting anim!");
        animator.SetTrigger("pressedTrigger");
    }

    public void playPressed() {
        print("play pressed in main menu!");
        SceneManager.LoadScene(1);
    }
}