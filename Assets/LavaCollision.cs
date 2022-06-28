using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaCollision : MonoBehaviour
{
    float coolDownAttaque;
    bool touch = false;
    private GameObject player, respawn;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        respawn = GameObject.FindGameObjectWithTag("Death");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            touch = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            touch = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(HealthBar.GetHealthBarValue() == 0f)
        {
            player.transform.position = respawn.transform.position;
            HealthBar.SetHealthBarValue(1f);
        }
        if(touch)
        {
            if (coolDownAttaque > 0)
                coolDownAttaque -= Time.deltaTime;
            if (coolDownAttaque < 0)
                coolDownAttaque = 0;

            if (coolDownAttaque == 0)
            {
                HealthBar.SetHealthBarValue(HealthBar.GetHealthBarValue() - 0.005f);
                //Debug.Log("Attaque");
                coolDownAttaque = 0.001f;
            }
        }
        
    }
}
