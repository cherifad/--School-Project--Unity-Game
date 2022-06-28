using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lavaKill : MonoBehaviour
{
    public GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colision");
        if (other.gameObject == player)
        {
            player.transform.position = new Vector3(0, 0, 0);//(where you want to teleport)
            Debug.Log("Colisionnnnnnnnnnnnnnnn");
        }
    }
}
