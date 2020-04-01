using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float scrollSpeed = 20f;

    private bool disabled;

    [Header("Bounds")] public Vector2 panLimit;

    // Update is called once per frame
    void Update() {

        if (Time.timeSinceLevelLoad > 14.5 && !disabled) {
            this.GetComponent<Animator>().enabled = false;
        }
        
        Vector3 pos = transform.position;
        var oldY = pos.y;

        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - panBorderThickness) {
            pos += panSpeed * Time.deltaTime * transform.up;
        }

        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panBorderThickness) {
            pos += panSpeed * Time.deltaTime * -transform.up;
        }

        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - panBorderThickness) {
            pos += panSpeed * Time.deltaTime * transform.right;
        }

        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panBorderThickness) {
            pos += panSpeed * Time.deltaTime * -transform.right;
        }

        pos.y = oldY;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos += Time.deltaTime * scroll * scrollSpeed * 100f * this.transform.forward;
        //pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}