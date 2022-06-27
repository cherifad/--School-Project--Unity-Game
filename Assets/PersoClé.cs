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
            SceneManager.LoadScene("KeyGrabe");
            key.SetActive(false);
        }

    }
    
}
