using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersoClé : MonoBehaviour
{
    public GameObject key;
   


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("key"))
        {
            Destroy(key);
            SceneManager.LoadScene("KeyGrabe");

        }
        if (other.gameObject.tag.Equals("triggerlava"))
        {
            SceneManager.LoadScene("LavaReveal");
        }
        if (other.gameObject.tag.Equals("final"))
        {
            SceneManager.LoadScene("Final");
        }

    }
    
}
