using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levier : MonoBehaviour
{
    public GameObject text;
   

    public float speed;
    public Transform door;
    public float maxOpenValue;
    public bool oppening = false;
    public bool closing = false;
    public float currentValue = 0;
    public int aa = 0;

   
    void OnTriggerEnter(Collider other)
    {
        

        if (other.gameObject.name == "Player")
        {
            


            if (aa == 1)
            {
                oppening = true;
                text.SetActive(true);
                aa -= 1;
            }
            
        }
    }
    void OnTriggerExit(Collider other)
    {

        if (other.gameObject.name == "Player")
        {
            if (aa == 0)
            {
                closing = true;
                text.SetActive(false);
            }
        }

            
        
    }
    // Start is called before the first frame update
    void Start()
    {
        text.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            text.SetActive(false);
            if (oppening == true)
            {
                OpenDoor();
            }
            
          

        }
    }


    void OpenDoor()
    {
        float movement = speed * Time.deltaTime * 500;
        currentValue += movement;

        if (currentValue <= maxOpenValue)
        {
            door.position = new Vector3(door.position.x, door.position.y - movement, door.position.z);
        }
        else
        {
            oppening = false;
            closing = true;
        }

    }

    /*void CloseDoor()
    {
        float movement = speed * Time.deltaTime * 200;
        currentValue -= movement;

        if (currentValue >= 0)
        {
            door.position = new Vector3(door.position.x, door.position.y - movement, door.position.z);
        }
        else
        {
            closing = false;
            oppening = true;
        }

    }*/
}
