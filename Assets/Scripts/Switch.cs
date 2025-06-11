using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public int type;
    [HideInInspector] public bool gun, hand2;
    public GameObject[] props;
    
    [Header("Temporary")]
    public Transform tip;
    public Transform tip2;
    public float range, range2, fireRate, fireRate2;
    Shooter shooter;

    private void Start() {
        foreach (GameObject prop in props) {
            if(prop != null)
                prop.SetActive(false);
        }
        if(props[type] != null)
            props[type].SetActive(true);
            shooter = GetComponent<Shooter>();
    }

    private void Update() {
        gun = (type == 2) || (type == 3);
        hand2 = gun && (type == 2);
        if(type == 2) {
            shooter.tip = tip; 
            shooter.range = range;
            shooter.fireRate = fireRate;
        }
        if(type == 3) {
            shooter.tip = tip2;
            shooter.range = range2;
            shooter.fireRate = fireRate2;
        }
        
        if(Input.GetKeyDown(KeyCode.KeypadPlus)) {
            Index((type == 4) ? 0: type + 1);
        } else if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
            Index((type == 0) ? 4: type - 1);
        }
    }

    public void Index(int id) {
        if(props[type] != null)
            props[type].SetActive(false);
        type = id;
        if(props[type] != null)
            props[type].SetActive(true);
    }
}
