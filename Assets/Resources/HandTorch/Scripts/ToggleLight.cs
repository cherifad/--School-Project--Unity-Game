using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLight : MonoBehaviour
{

    public GameObject Spot1ight;

    public bool onOff;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (onOff)
        {
            Spot1ight.SetActive(true);
        }
        else {
            Spot1ight.SetActive(false);
        }
        toggle();
    }

    public void toggle ()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            onOff = !onOff;
        }
    }
}
