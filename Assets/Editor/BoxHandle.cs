using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [CustomEditor(typeof(ObjectPlacement))]
    public class WireBoxExample : Editor
    {
        void OnSceneGUI()
        {
            Handles.color = Color.yellow;
            ObjectPlacement myObj = (ObjectPlacement)target;
            Handles.DrawWireCube(myObj.buildAreaCenter, myObj.buildAreaSize);
        }
    }
}
