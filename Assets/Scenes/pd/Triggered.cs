using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggered : MonoBehaviour
{
    

    public float speed;
    public Transform door;
    public float maxOpenValue;
    public bool oppening = false;
    public bool closing = false;
    public float currentValue = 0;


    void Start()
    {
        
    }
    // Start is called before the first frame update
    void Update()
    {

            if (oppening == true)
            {
                OpenDoor();
            }
            if (closing == true)
            {
                CloseDoor();
            }
        

    }

    private void OnTriggerEnter(Collider other)
    {

        oppening = true;
        
       
    }

    void OnTriggerExit(Collider other)
    {

        if (other.transform.name == "Player")
        {
            oppening = false;
            closing = true;
        }
    }

    void OpenDoor()
    {
        float movement = speed * Time.deltaTime;
        currentValue += movement;

        if (currentValue <= maxOpenValue)
        {
            door.position = new Vector3(door.position.x , door.position.y - movement, door.position.z);
        }
        else
        {
            oppening = false;
            //closing = true;
        }

    }

    void CloseDoor()
    {
        float movement = speed * Time.deltaTime;
        currentValue -= movement;

        if (currentValue >= 0)
        {
            door.position = new Vector3(door.position.x, door.position.y + movement, door.position.z);
        }
        else
        {
            closing = false;
            //oppening = true;
        }

    }




}
