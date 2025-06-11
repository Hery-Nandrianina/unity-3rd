using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    Text txt;

    private void Start() {
        txt = GetComponent<Text>();
        // txt.text = "None";
    }

    private void OnGUI() {
        Event e = Event.current;
        if (e.isKey)
        {
            if(e.keyCode.ToString() != "None")
                txt.text = e.keyCode.ToString();
        }
    }
}
