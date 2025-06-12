using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Helper : MonoBehaviour
{
    public Image targetImage; // Assign in Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (targetImage != null)
            {
                targetImage.gameObject.SetActive(!targetImage.gameObject.activeSelf);
            }
        }
    }
}
