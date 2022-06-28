using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save : MonoBehaviour
{
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");

        if (objs.Length > 1)
        {
            //Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
