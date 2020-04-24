using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorFunctions animatorFunctions;
    [SerializeField] int thisIndex;
    [SerializeField] GameLoader gameLoader;

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
                    switch (menuButtonController.index) {
                        //Play Button
                        case 0:
                            playPressed();
                            break;
                        //Options Button
                        case 1:
                            optionsPressed();
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
        animator.SetTrigger("pressedTrigger");
    }

    public void playPressed() {
        Debug.Log("playPressed");
        gameLoader.LoadGame();
    }

    public void optionsPressed()
    {
        Debug.Log("optionsPressed");
        gameLoader.LoadScene(2);
    }
}